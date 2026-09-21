using RRSOS_PCC.Classes;
using System.Text;

namespace RRSOS_PCC.Services
{
    public sealed record ResupplyReport(
        bool Success,
        string? Error,
        IReadOnlyList<ResupplyLine> Lines,
        string? BackupPath,
        DateTime At)
    {
        public int TotalAdded => Lines.Sum(l => l.Added);
    }

    /// <summary>
    /// The RESUPPLY button: tops up every container labelled DaveFood, DaveWater and DaveOxygen in
    /// the selected save, by editing the save file on disk (see <see cref="SaveResupplier"/>).
    ///
    /// The game keeps its world in memory and rewrites the file itself, so this only sticks while the
    /// game is at its main menu or closed. Every run first stores a byte-exact backup of the save, and
    /// nothing is written if the file changes while this is working.
    /// </summary>
    public class ResupplyService
    {
        /// <summary>Container label to look for, and the item that fills it. Labels are matched ignoring case.</summary>
        public static readonly IReadOnlyList<ResupplyTarget> Targets = new[]
        {
            new ResupplyTarget("DaveFood", "astrofood", "Food"),
            new ResupplyTarget("DaveWater", "WaterBottle1", "Water"),
            new ResupplyTarget("DaveOxygen", "OxygenCapsule1", "Oxygen")
        };

        /// <summary>The short name to show for a label, e.g. "Food" for "DaveFood".</summary>
        public static string DisplayName(string label) =>
            Targets.FirstOrDefault(t => string.Equals(t.Label, label, StringComparison.OrdinalIgnoreCase))?.Display ?? label;

        // Old backups are pruned so a busy session does not fill the disk.
        private const int BackupsToKeep = 20;

        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly SaveService _save;

        public ResupplyService(SaveService save)
        {
            _save = save;
        }

        public async Task<ResupplyReport> ResupplyAsync()
        {
            await _gate.WaitAsync();
            try
            {
                var report = await Task.Run(Run);

                // Pick the new contents up straight away instead of waiting for the next poll.
                if (report.Success && report.TotalAdded > 0)
                    await _save.Load();

                return report;
            }
            finally
            {
                _gate.Release();
            }
        }

        private static ResupplyReport Run()
        {
            var at = DateTime.Now;
            var path = PathResolver.FullSavePath;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return Fail("No save file is selected.", at);

            try
            {
                var before = Signature(path);
                var original = ReadShared(path);

                var hasBom = original.Length >= 3 && original[0] == 0xEF && original[1] == 0xBB && original[2] == 0xBF;
                var text = Encoding.UTF8.GetString(original, hasBom ? 3 : 0, original.Length - (hasBom ? 3 : 0));

                var outcome = SaveResupplier.Apply(text, Targets);

                if (outcome.Failed)
                    return new ResupplyReport(false, "Nothing was changed. " + string.Join(" ", outcome.Problems), outcome.Lines, null, at);

                if (!outcome.Changed)
                    return new ResupplyReport(true, null, outcome.Lines, null, at);

                // Keep the original before anything is written.
                var backup = WriteBackup(path, original, at);

                var edited = (hasBom ? new byte[] { 0xEF, 0xBB, 0xBF } : Array.Empty<byte>())
                    .Concat(Encoding.UTF8.GetBytes(outcome.NewText!))
                    .ToArray();

                // Write beside the save and swap it in, so a crash can never leave half a save behind.
                var temp = path + ".resupply.tmp";
                File.WriteAllBytes(temp, edited);

                try
                {
                    if (Signature(path) != before)
                        return new ResupplyReport(false, "The save changed while resupplying, so nothing was written. Is a game still running a world? Try again from the main menu.", outcome.Lines, backup, at);

                    File.Move(temp, path, overwrite: true);
                }
                finally
                {
                    if (File.Exists(temp))
                        File.Delete(temp);
                }

                PruneBackups(path);
                return new ResupplyReport(true, null, outcome.Lines, backup, at);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return Fail("Could not update the save file: " + ex.Message, at);
            }
        }

        private static ResupplyReport Fail(string error, DateTime at) =>
            new(false, error, Array.Empty<ResupplyLine>(), null, at);

        private static (long Length, DateTime LastWriteUtc) Signature(string path)
        {
            var info = new FileInfo(path);
            return (info.Length, info.LastWriteTimeUtc);
        }

        /// <summary>Reads while the game may still have the file open.</summary>
        private static byte[] ReadShared(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }

        private static string BackupFolder => Path.Combine(PathResolver.UserDataPath, "save-backups");

        private static string WriteBackup(string savePath, byte[] original, DateTime at)
        {
            Directory.CreateDirectory(BackupFolder);
            var name = Path.GetFileNameWithoutExtension(savePath);
            var backup = Path.Combine(BackupFolder, $"{name}.{at:yyyyMMdd-HHmmss}.json");
            File.WriteAllBytes(backup, original);
            return backup;
        }

        private static void PruneBackups(string savePath)
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(savePath);
                var old = new DirectoryInfo(BackupFolder)
                    .EnumerateFiles($"{name}.*.json")
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .Skip(BackupsToKeep);

                foreach (var file in old)
                    file.Delete();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Leaving an old backup behind is harmless.
            }
        }
    }
}
