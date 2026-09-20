using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RRSOS_PCC.Services
{
    public class WorldObjectClassifierService
    {
        private static readonly Regex TrailingDigits = new(@"\d+$", RegexOptions.Compiled);

        private readonly Dictionary<string, WorldObjectDefinition> _definitions = new();

        // A save has thousands of objects but only a few hundred distinct gIds.
        private readonly ConcurrentDictionary<string, WorldObjectDefinition> _resolved = new();

        public WorldObjectClassifierService()
        {
            LoadDefinitions();
        }

        private void LoadDefinitions()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    // Allows "Machine" -> BaseType.Machine
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                };

                string path = PathResolver.WorldObjectDataPath;

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var data = JsonSerializer.Deserialize<Dictionary<string, WorldObjectDefinition>>(json, options);

                    if (data != null)
                    {
                        foreach (var kvp in data)
                        {
                            _definitions[kvp.Key] = kvp.Value;
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine($"[WorldObjectClassifier] {path} not found; every object will be Unknown.");
                }
            }
            catch (Exception ex)
            {
                // Don't crash the app if the JSON is malformed
                Console.Error.WriteLine($"[WorldObjectClassifier] Error loading worldobjectdata.json: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the metadata for a given gId.
        /// Handles stripping trailing numbers (e.g., 'Iron1' -> 'Iron')
        /// if an exact match isn't found.
        /// </summary>
        public WorldObjectDefinition GetDefinition(string gId)
        {
            if (string.IsNullOrEmpty(gId))
                return CreateUnknown(gId);

            return _resolved.GetOrAdd(gId, Resolve);
        }

        private WorldObjectDefinition Resolve(string gId)
        {
            // 1. Exact match (e.g., "VegetableGrower1")
            if (_definitions.TryGetValue(gId, out var exactMatch))
                return exactMatch;

            // 2. Base name match (strip trailing numbers)
            string baseId = TrailingDigits.Replace(gId, "");
            if (_definitions.TryGetValue(baseId, out var baseMatch))
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
