using Microsoft.Extensions.Diagnostics.HealthChecks;
using RRSOS_PCC.Enums;

namespace RRSOS_PCC.ViewModels
{
    public class ExtractorSummaryVM
    {
        public long Id { get; init; }
        public ExtractorType Type { get; init; }          // "Ore", "Algae", "Water"
        public string ProductGroup { get; init; }  // Iridium, Algae, WaterBottle

        public int PrimaryCount { get; init; }     // Main ore count (or Count for Water/Algae)
        public int Count { get; init; }
        public int Capacity { get; init; }
        public bool IsFull { get; init; }

        public string Direction { get; init; }
        public string Position2D { get; init; }

        public float Distance { get; init; }

        public Dictionary<string, int> GroupedContents { get; init; } // Ore only
    }
}
