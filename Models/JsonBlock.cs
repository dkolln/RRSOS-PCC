using RRSOS_PCC.Classes;

namespace RRSOS_PCC.Models
{
    public class JsonBlock
    {
        public JsonBlock(string raw)
        {
            Raw = raw.Trim();

            // Only a '|' outside a JSON string separates entries; sign/note/player text may contain one.
            var parts = SaveSplitter.Split(Raw, '|', out var sawSeparator);

            IsMultiEntry = sawSeparator;
            Entries = sawSeparator
                ? parts.Select(e => e.Trim()).Where(e => e.Length > 0).ToList()
                : new List<string>();
        }

        public string Raw { get; }
        public bool IsMultiEntry { get; }
        public List<string> Entries { get; }
    }

}
