using RRSOS_PCC.Enums;

namespace RRSOS_PCC.Models
{
    public class WorldObjectDataEntry
    {
        public string gId { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Manual { get; set; }

        public string Tier { get; set; } = "";
        public WorldObjectType Type { get; set; }
    }

}
