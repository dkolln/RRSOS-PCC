using RRSOS_PCC.Enums;
using System.Text.Json.Serialization;

namespace RRSOS_PCC.Models
{
    public class WorldObjectDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = "T1";

        /// <summary>
        /// Power per machine in kW: positive makes power (for a generator, its base output before
        /// optimizer boosts), negative uses it, absent means unknown
        /// (not "zero"). Tiers differ, so this is only ever read from an exact gId entry.
        /// </summary>
        public decimal? PowerKw { get; set; }

        /// <summary>
        /// Name of the little picture used for this machine in the power card ("windmill", "sun",
        /// "sun-large", "atom", "atoms", "starburst"). Absent means the plain default.
        /// </summary>
        public string? Icon { get; set; }

        public WorldObjectType Type { get; set; }

        public WorldObjectCategory Category { get; set; }

        public bool IsContainer { get; set; }
        public bool IsGrower { get; set; }
        public bool IsExtractor { get; set; }
        public bool IsStorage { get; set; }
        public bool IsSingleItemContainer { get; set; }
    }
}
