namespace RRSOS_PCC.Models
{
    public class ItemSearchResult
    {
        public long ItemID { get; set; } = 0;
        public string ItemName { get; set; } = "";
        public string BaseName { get; set; } = "";
        public string BasePos { get; set; } = "";
        public int Distance { get; set; }

        public string Direction { get; set; } = "";
    }

}
