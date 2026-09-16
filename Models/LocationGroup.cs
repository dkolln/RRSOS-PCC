namespace RRSOS_PCC.Models
{
    public class LocationGroup
    {
        public string LocationName { get; set; }
        public int Count { get; set; }
        public int NearestDistance { get; set; }
        public string Direction { get; set; }
        public string BasePos { get; set; }
    }
}
