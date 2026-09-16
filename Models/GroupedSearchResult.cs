namespace RRSOS_PCC.Models
{
    public class GroupedSearchResult
    {
        public string ItemName { get; set; }
        public List<LocationGroup> Locations { get; set; } = new();
    }
}
