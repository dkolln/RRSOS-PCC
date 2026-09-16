using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RRSOS_PCC.Services
{
    public class WorldObjectClassifierService
    {
        private readonly Dictionary<string, WorldObjectDefinition> _definitions = new();

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
                    // This is the magic line that allows "Machine" -> BaseType.Machine
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                };

                // Assuming the JSON is in your assets folder
                string path = Path.Combine(PathResolver.AssetPath, "worldobjectdata.json");

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
            }
            catch (Exception ex)
            {
                // Fallback or log error: You don't want the app to crash if the JSON is malformed
                Console.WriteLine($"Error loading worldobjectdata.json: {ex.Message}");
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

            // 1. Try exact match (e.g., "VegetableGrower1")
            if (_definitions.TryGetValue(gId, out var exactMatch))
                return exactMatch;

            // 2. Try base name match (strip trailing numbers)
            string baseId = System.Text.RegularExpressions.Regex.Replace(gId, @"\d+$", "");
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

        private WorldObjectDefinition CreateUnknown(string gId)
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