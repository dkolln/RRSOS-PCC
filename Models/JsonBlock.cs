namespace RRSOS_PCC.Models
{
    public class JsonBlock
    {
        public JsonBlock(string raw)
        {
            Raw = raw.Trim();

            if (IsMultiEntry)
            {
                Entries = raw
                    .Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToList();
            }
            else
            {
                Entries = new List<string>();
            }
        }

        public string Raw { get; }
        public bool IsMultiEntry => Raw.Contains('|');
        public List<string> Entries { get; }
    }

}
