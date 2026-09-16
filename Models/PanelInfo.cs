namespace RRSOS_PCC.Models
{
    public class PanelInfo
    {
        public string Direction { get; set; }           // N, NE, E, SE, S, SW, W, NW
        public string Module { get; set; }              // Door, Wall, Window, Connected, etc.

        public string Type { get; set; }
    }
}
