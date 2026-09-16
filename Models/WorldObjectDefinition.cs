using RRSOS_PCC.Enums;
using System.Text.Json.Serialization;

namespace RRSOS_PCC.Models
{
    public class WorldObjectDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = "T1";

        public WorldObjectType Type { get; set; }

        public WorldObjectCategory Category { get; set; }

        public bool IsContainer { get; set; }
        public bool IsGrower { get; set; }
        public bool IsExtractor { get; set; }
        public bool IsStorage { get; set; }
        public bool IsSingleItemContainer { get; set; }
    }
}
