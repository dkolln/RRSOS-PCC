namespace RRSOS_PCC.ViewModels
{
    public class ExtractorSubGroupVM
    {
        public string ProductGroup { get; set; }

        public int TotalItems { get; set; }
        public int FullExtractors { get; set; }
        public int TotalExtractors { get; set; }

        public List<ExtractorSummaryVM> Extractors { get; set; } = new();
    }

}

