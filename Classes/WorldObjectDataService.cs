using RRSOS_PCC.Models;
using System.ComponentModel;
using System.Text.Json;
using Cont = RRSOS_PCC.Models.Container;

namespace RRSOS_PCC.Classes
{
    public static class WorldObjectDataService
    {
        private static readonly Dictionary<string, WorldObjectDataEntry> _entries =
        new(StringComparer.OrdinalIgnoreCase);

        private static string DataFilePath => PathResolver.WorldObjectDataPath;

        public static IReadOnlyDictionary<string, WorldObjectDataEntry> Entries => _entries;

        // ------------------------------------------------------------
        // Load
        // ------------------------------------------------------------
        public static void Load()
        {
            _entries.Clear();

            if (!File.Exists(DataFilePath))
                return;

            var json = File.ReadAllText(DataFilePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, WorldObjectDataEntry>>(json);

            if (dict == null)
                return;

            _entries.Clear();

            foreach (var kvp in dict)
            {
                // The key is the gId ("VegetableGrower1")
                var entry = kvp.Value;
                entry.gId = kvp.Key;   // ensure gId is set

                _entries[entry.gId] = entry;
            }
        }

        // ------------------------------------------------------------
        // Save
        // ------------------------------------------------------------
        public static void Save()
        {
            var json = JsonSerializer.Serialize(
                _entries.Values.OrderBy(e => e.gId),
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(DataFilePath, json);
        }

        // ------------------------------------------------------------
        // Lookup
        // ------------------------------------------------------------
        public static WorldObjectDataEntry? GetEntry(string gId)
        {
            if (_entries.TryGetValue(gId, out var entry))
                return entry;

            return null;
        }

        // ------------------------------------------------------------
        // Add or update (used after user assigns a type)
        // ------------------------------------------------------------
        public static void Upsert(WorldObjectDataEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.gId))
                return;

            _entries[entry.gId] = entry;
        }
    }
}
