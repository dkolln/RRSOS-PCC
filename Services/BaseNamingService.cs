using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RRSOS_PCC.Services
{
    /// <summary>
    /// Names bases and outposts. Priority: sign text > saved name (basedata.json) > next name
    /// from the procedural pool. Procedural names are saved so they stay put between loads.
    /// Thread-safe; the file is only written when something actually changed (FlushIfDirty).
    /// </summary>
    public class BaseNamingService
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true,
            // Shows the BaseType enum as "Base"/"Outpost" in the JSON
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly object _lock = new();

        // Saved names: manual ones (from signs) and procedural ones (Manual = false)
        private Dictionary<string, BaseDataEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

        // Procedural pools from JSON
        private List<string> _basePool = new();
        private List<string> _outpostPool = new();

        // Names handed out this session
        private readonly Dictionary<long, string> _sessionAssignments = new();

        private bool _dirty;

        public BaseNamingService()
        {
            LoadMetadata();
        }

        private void LoadMetadata()
        {
            _entries = ReadJson<Dictionary<string, BaseDataEntry>>(PathResolver.BaseDataPath, keepCorruptCopy: true)
                       ?? new(StringComparer.OrdinalIgnoreCase);

            // Case-insensitive lookups regardless of how the file was written
            _entries = new Dictionary<string, BaseDataEntry>(_entries, StringComparer.OrdinalIgnoreCase);

            _basePool = ReadJson<List<string>>(PathResolver.BaseNamesPath) ?? new();
            _outpostPool = ReadJson<List<string>>(PathResolver.OutpostNamesPath) ?? new();
        }

        private static T? ReadJson<T>(string path, bool keepCorruptCopy = false) where T : class
        {
            if (!File.Exists(path))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"[BaseNamingService] {Path.GetFileName(path)} is malformed: {ex.Message}");

                // Never let a later save silently overwrite the user's names.
                if (keepCorruptCopy)
                    File.Copy(path, path + ".corrupt", overwrite: true);

                return null;
            }
        }

        public string GetBaseName(Base b, string? signText = null)
        {
            lock (_lock)
            {
                string idString = b.id.ToString();

                // TIER 1: The sign (the registration tool)
                if (!string.IsNullOrWhiteSpace(signText))
                {
                    if (!_entries.TryGetValue(idString, out var manualEntry)
                        || manualEntry.Name != signText
                        || !manualEntry.Manual)
                    {
                        _entries[idString] = new BaseDataEntry
                        {
                            Name = signText,
                            Manual = true,
                            Type = b.Type
                        };
                        _dirty = true;
                    }
                    return signText;
                }

                // TIER 2: Saved record (matches even if the sign is gone)
                if (_entries.TryGetValue(idString, out var persistentEntry))
                    return persistentEntry.Name;

                // TIER 3: Assigned earlier this session
                if (_sessionAssignments.TryGetValue(b.id, out var sessionName))
                    return sessionName;

                // TIER 4: New procedural name, saved so it survives restarts
                string newName = AssignProceduralName(b);
                _sessionAssignments[b.id] = newName;

                _entries[idString] = new BaseDataEntry
                {
                    Name = newName,
                    Manual = false,   // so it can be cleaned up if the base disappears
                    Type = b.Type
                };
                _dirty = true;

                return newName;
            }
        }

        private string AssignProceduralName(Base b)
        {
            var pool = (b.Type == BaseType.Base) ? _basePool : _outpostPool;

            // A name is taken if it was handed out this session OR is already saved for any
            // base (saved names are not in the session map after a restart).
            var taken = new HashSet<string>(_entries.Values.Select(e => e.Name), StringComparer.OrdinalIgnoreCase);
            taken.UnionWith(_sessionAssignments.Values);

            var unusedName = pool.FirstOrDefault(name => !taken.Contains(name));
            if (unusedName != null)
                return unusedName;

            // Pool exhausted: fall back to the last 4 digits of the id
            var id = b.id.ToString();
            return id.Length > 4 ? id[^4..] : id;
        }

        /// <summary>
        /// Drops saved procedural names for bases that no longer exist. Manual names are kept.
        /// Does nothing when the current save has no bases (a stub or empty save must not wipe names).
        /// </summary>
        public void CleanupOrphanedEntries(List<Base> activeBases)
        {
            if (activeBases == null || activeBases.Count == 0)
                return;

            lock (_lock)
            {
                var activeIds = activeBases.Select(b => b.id.ToString()).ToHashSet();

                var keysToRemove = _entries
                    .Where(kvp => !activeIds.Contains(kvp.Key) && !kvp.Value.Manual)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _entries.Remove(key);

                    // Free the name for reuse right away
                    if (long.TryParse(key, out long numericId))
                        _sessionAssignments.Remove(numericId);

                    _dirty = true;
                }
            }
        }

        /// <summary>Writes basedata.json if (and only if) something changed since the last write.</summary>
        public void FlushIfDirty()
        {
            lock (_lock)
            {
                if (!_dirty)
                    return;

                try
                {
                    var path = PathResolver.BaseDataPath;
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    // Write-then-move so a crash mid-write cannot leave a truncated file.
                    var temp = path + ".tmp";
                    File.WriteAllText(temp, JsonSerializer.Serialize(_entries, WriteOptions));
                    File.Move(temp, path, overwrite: true);

                    _dirty = false;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // Stays dirty; the next load tries again.
                    Console.Error.WriteLine($"[BaseNamingService] Could not save basedata.json: {ex.Message}");
                }
            }
        }
    }
}
