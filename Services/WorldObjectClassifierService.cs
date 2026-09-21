using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RRSOS_PCC.Services
{
    /// <summary>
    /// Looks up what a gId is (name, tier, type, power rate) from worldobjectdata.json.
    ///
    /// The file is watched: save it and the new definitions are live within a moment, no restart
    /// needed. A file that is malformed (say, caught mid-edit) is ignored and the previous
    /// good definitions stay in use. <see cref="DefinitionsChanged"/> fires after a successful
    /// reload so the owner of parsed data can rebuild it, since names, tiers and power rates are
    /// copied onto objects when a save is parsed.
    /// </summary>
    public class WorldObjectClassifierService : IDisposable
    {
        private static readonly Regex TrailingDigits = new(@"\d+$", RegexOptions.Compiled);

        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            // Allows "Machine" -> BaseType.Machine
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        // Editors write a file in several steps and can briefly hold it open; wait for the dust to settle.
        private static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(400);
        private const int ReadAttempts = 4;
        private static readonly TimeSpan ReadRetryDelay = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// One immutable generation of the data. It is replaced whole on reload, so a reader that
        /// grabbed it keeps a consistent view, and the per-gId cache goes away with it.
        /// </summary>
        private sealed class Snapshot
        {
            // The game is not consistent about gId casing ("Biodome2" in a save, "biodome2" in the data file).
            public Dictionary<string, WorldObjectDefinition> Definitions { get; }

            // A save has thousands of objects but only a few hundred distinct gIds.
            public ConcurrentDictionary<string, WorldObjectDefinition> Resolved { get; } = new();

            public Snapshot(Dictionary<string, WorldObjectDefinition> definitions) => Definitions = definitions;
        }

        private volatile Snapshot _snapshot = new(new(StringComparer.OrdinalIgnoreCase));

        private readonly object _reloadLock = new();
        private FileSystemWatcher? _watcher;
        private Timer? _debounceTimer;

        /// <summary>Raised (on a background thread) after worldobjectdata.json was reloaded with new content.</summary>
        public event Action? DefinitionsChanged;

        public WorldObjectClassifierService()
        {
            var path = PathResolver.WorldObjectDataPath;

            if (!File.Exists(path))
                Console.Error.WriteLine($"[WorldObjectClassifier] {path} not found; every object will be Unknown until it exists.");
            else
                TryReload(path);

            StartWatching(path);
        }

        // ------------------------------------------------------------------
        // Loading and watching
        // ------------------------------------------------------------------

        /// <returns>True if a valid file was read and the definitions were replaced.</returns>
        private bool TryReload(string path)
        {
            lock (_reloadLock)
            {
                Dictionary<string, WorldObjectDefinition>? data = null;
                Exception? failure = null;

                for (var attempt = 1; attempt <= ReadAttempts; attempt++)
                {
                    try
                    {
                        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                        data = JsonSerializer.Deserialize<Dictionary<string, WorldObjectDefinition>>(stream, ReadOptions);
                        failure = null;
                        break;
                    }
                    catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
                    {
                        // Locked by the editor, or only half written so far: try again shortly.
                        failure = ex;
                        if (attempt < ReadAttempts)
                            Thread.Sleep(ReadRetryDelay);
                    }
                }

                if (data == null)
                {
                    Console.Error.WriteLine(failure == null
                        ? "[WorldObjectClassifier] worldobjectdata.json is empty; keeping the previous definitions."
                        : $"[WorldObjectClassifier] Could not read worldobjectdata.json, keeping the previous definitions: {failure.Message}");
                    return false;
                }

                var definitions = new Dictionary<string, WorldObjectDefinition>(data, StringComparer.OrdinalIgnoreCase);
                _snapshot = new Snapshot(definitions);

                Console.WriteLine($"[WorldObjectClassifier] Loaded {definitions.Count} definitions from worldobjectdata.json.");
                return true;
            }
        }

        private void StartWatching(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return;

            _debounceTimer = new Timer(_ => OnFileSettled(path), null, Timeout.Infinite, Timeout.Infinite);

            _watcher = new FileSystemWatcher(directory, Path.GetFileName(path))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            // Saving often means "write a temp file, then rename it over the original".
            _watcher.Changed += (_, _) => ScheduleReload();
            _watcher.Created += (_, _) => ScheduleReload();
            _watcher.Renamed += (_, _) => ScheduleReload();
            _watcher.Error += (_, e) => Console.Error.WriteLine($"[WorldObjectClassifier] File watcher error: {e.GetException().Message}");

            _watcher.EnableRaisingEvents = true;
        }

        // Restarting the timer on every event turns a burst of them into one reload.
        private void ScheduleReload() => _debounceTimer?.Change(Debounce, Timeout.InfiniteTimeSpan);

        private void OnFileSettled(string path)
        {
            try
            {
                if (File.Exists(path) && TryReload(path))
                    DefinitionsChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WorldObjectClassifier] Reload failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _debounceTimer?.Dispose();
        }

        // ------------------------------------------------------------------
        // Lookups
        // ------------------------------------------------------------------

        /// <summary>
        /// Retrieves the metadata for a given gId.
        /// Handles stripping trailing numbers (e.g., 'Iron1' -> 'Iron')
        /// if an exact match isn't found.
        /// </summary>
        public WorldObjectDefinition GetDefinition(string gId)
        {
            if (string.IsNullOrEmpty(gId))
                return CreateUnknown(gId);

            var snapshot = _snapshot;
            return snapshot.Resolved.GetOrAdd(gId, static (id, s) => Resolve(s, id), snapshot);
        }

        /// <summary>
        /// The entry for exactly this gId, with none of <see cref="GetDefinition"/>'s trailing-digit
        /// fallback. Use this for values that differ by tier (like power), where borrowing
        /// another tier's entry would give a wrong answer instead of an honest "unknown".
        /// </summary>
        public bool TryGetExactDefinition(string gId, out WorldObjectDefinition definition)
        {
            if (!string.IsNullOrEmpty(gId) && _snapshot.Definitions.TryGetValue(gId, out var found))
            {
                definition = found;
                return true;
            }

            definition = null!;
            return false;
        }

        private static WorldObjectDefinition Resolve(Snapshot snapshot, string gId)
        {
            // 1. Exact match (e.g., "VegetableGrower1")
            if (snapshot.Definitions.TryGetValue(gId, out var exactMatch))
                return exactMatch;

            // 2. Base name match (strip trailing numbers)
            string baseId = TrailingDigits.Replace(gId, "");
            if (snapshot.Definitions.TryGetValue(baseId, out var baseMatch))
                return baseMatch;

            // 3. Absolute fallback
            return CreateUnknown(gId);
        }

        /// <summary>
        /// True if gId (or its base name with trailing digits stripped) has a
        /// real entry in worldobjectdata.json, using the same fallback as GetDefinition.
        /// </summary>
        public bool IsKnownGid(string gId) => GetDefinition(gId).Type != WorldObjectType.Unknown;

        private static WorldObjectDefinition CreateUnknown(string gId)
        {
            return new WorldObjectDefinition
            {
                Name = gId,
                Type = WorldObjectType.Unknown,
                Category = WorldObjectCategory.Unknown
            };
        }
    }
}
