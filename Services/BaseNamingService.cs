using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Text.Json;

namespace RRSOS_PCC.Services
{
    public class BaseNamingService
    {
        private readonly string _dataFilePath;
        private readonly string _baseNamesPath;
        private readonly string _outpostNamesPath;

        // The high-priority manual overrides from basedata.json
        private Dictionary<string, BaseDataEntry> _manualEntries = new(StringComparer.OrdinalIgnoreCase);

        // Procedural pools from JSON
        private List<string> _basePool = new();
        private List<string> _outpostPool = new();

        // Keeps procedural assignments consistent during a session
        private readonly Dictionary<long, string> _sessionAssignments = new();

        public BaseNamingService(IConfiguration config)
        {
            var assetPath = config["SaveSettings:AssetPath"] ?? "Assets";
            _dataFilePath = Path.Combine(assetPath, "basedata.json");
            _baseNamesPath = Path.Combine(assetPath, "basenames.json");
            _outpostNamesPath = Path.Combine(assetPath, "outpostnames.json");

            LoadMetadata();
        }

        private void LoadMetadata()
        {
            // Load Manual Overrides
            if (File.Exists(_dataFilePath))
            {
                var json = File.ReadAllText(_dataFilePath);
                _manualEntries = JsonSerializer.Deserialize<Dictionary<string, BaseDataEntry>>(json) ?? new();
            }

            // Load Procedural Lists
            if (File.Exists(_baseNamesPath))
                _basePool = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_baseNamesPath)) ?? new();

            if (File.Exists(_outpostNamesPath))
                _outpostPool = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_outpostNamesPath)) ?? new();
        }

        public string GetBaseName(Base b, string? signText = null)
        {
            string idString = b.id.ToString();

            // TIER 1: The Sign (The Registration Tool)
            if (!string.IsNullOrWhiteSpace(signText))
            {
                // Renamed 'existing' to 'manualEntry' to avoid CS0136
                if (!_manualEntries.TryGetValue(idString, out var manualEntry) || manualEntry.Name != signText)
                {
                    _manualEntries[idString] = new BaseDataEntry
                    {
                        Name = signText,
                        Manual = true,
                        Type = b.Type
                    };
                    Save();
                }
                return signText;
            }

            // TIER 2: Persistent Record (Matches even if the sign is gone)
            if (_manualEntries.TryGetValue(idString, out var persistentEntry))
            {
                return persistentEntry.Name;
            }

            // TIER 3: Session-Persistent Procedural Naming
            // Renamed 'existing' to 'sessionName' to avoid conflict
            if (_sessionAssignments.TryGetValue(b.id, out var sessionName))
            {
                return sessionName;
            }

            // TIER 4: New Procedural Assignment + Persistence
            string newName = AssignProceduralName(b);
            _sessionAssignments[b.id] = newName;

            // Option A: Promote to JSON as well (Manual = false so it can be cleaned up)
            _manualEntries[idString] = new BaseDataEntry
            {
                Name = newName,
                Manual = false,
                Type = b.Type
            };

            Save();

            return newName;
        }

        private string AssignProceduralName(Base b)
        {
            var pool = (b.Type == BaseType.Base) ? _basePool : _outpostPool;
            //var prefix = (b.Type == BaseType.Base) ? "Base" : "Outpost";

            // Find the first name in the pool not already assigned this session
            var unusedName = pool.FirstOrDefault(name => !_sessionAssignments.Values.Contains(name));

            //return unusedName ?? $"{prefix} {b.id.ToString().AsSpan(^4)}"; // Use last 4 digits of ID if pool empty
            return unusedName ?? $"{b.id.ToString().AsSpan(^4)}"; // Use last 4 digits of ID if pool empty
        }

        private void Save()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                // Ensures the BaseType enum shows as "Base"/"Outpost" in the JSON
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

                var json = JsonSerializer.Serialize(_manualEntries, options);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                // Log it so you know if there's a file lock issue during the 60s sync
                System.Diagnostics.Debug.WriteLine($"NamingService Save Error: {ex.Message}");
            }
        }

        public void CleanupOrphanedEntries(List<Base> activeBases)
        {
            if (activeBases == null) return;

            // Use a HashSet for O(1) lookups during the filter
            var activeIds = activeBases.Select(b => b.id.ToString()).ToHashSet();
            bool needsSave = false;

            // 1. Identify "Ghost" records: 
            // They are in our dictionary but NOT in the game world, and they weren't manually named.
            var keysToRemove = _manualEntries
                .Where(kvp => !activeIds.Contains(kvp.Key) && !kvp.Value.Manual)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _manualEntries.Remove(key);

                // 2. Also free it from the session memory so the name can be reused immediately
                if (long.TryParse(key, out long numericId))
                {
                    _sessionAssignments.Remove(numericId);
                }

                needsSave = true;
            }

            // 3. Only hit the disk if we actually changed something
            if (needsSave)
            {
                Save();
            }
        }
    }
}