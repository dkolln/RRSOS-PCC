using RRSOS_PCC.Classes;

namespace RRSOS_PCC.Models
{
    public class Container : WorldObject
    {
        // Raw JSON fields
        public long liId { get; set; }
        public string siIds { get; set; }

        // Domain fields
        public List<WorldObject> PrimaryItems { get; set; } = new();

        public List<WorldObject> SecondaryItems { get; set; } = new();

        public int Capacity { get; set; }
    }
}
