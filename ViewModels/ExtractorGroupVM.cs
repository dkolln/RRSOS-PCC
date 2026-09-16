using RRSOS_PCC.Enums;

namespace RRSOS_PCC.ViewModels
{
    public class ExtractorGroupVM
    {
        public ExtractorType Type { get; set; }
        public string DisplayName { get; set; }

        public int TotalItems { get; set; }
        public int FullExtractors { get; set; }
        public int TotalExtractors { get; set; }

        // All extractors in this group (Ore, Water Life, etc.)
        public List<ExtractorSummaryVM> Extractors { get; set; } = new();

        // Only used for Ore extractors (Sulfur, Titanium, etc.)
        public List<ExtractorSubGroupVM>? SubGroups { get; set; }
    }
}
