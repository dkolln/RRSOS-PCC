using System.Text.Json.Serialization;
using RRSOS_PCC.Enums;

namespace RRSOS_PCC.Models
{
    public class BaseDataEntry
    {
        // The "True Name" you want displayed on the HUD (e.g., "Uranium Mine")
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // A flag to ensure the service knows this was a user-defined name
        [JsonPropertyName("manual")]
        public bool Manual { get; set; } = true;

        // Optional: Could store "Base" or "Outpost" to help the Binder
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BaseType Type { get; set; }
    }
}
