using System.Numerics;

namespace RRSOS_PCC.Models
{
    public class WaterCollector
    {
        public long id { get; set; }
        public string gId { get; set; }
        public int liId { get; set; }
        public string liGrps { get; set; }
        public string pos { get; set; }
        public string rot { get; set; }
        public int planet { get; set; }

        public Position Position { get; set; } = new Position("0,0,0");
        public Rotation Rotation { get; set; } = new Rotation("0,0,0,1");

        public List<WorldObject> Items { get; set; } = new();

        // Hydrate after deserialization
        public void Hydrate()
        {
            if (!string.IsNullOrWhiteSpace(pos))
                Position = new Position(pos);

            if (!string.IsNullOrWhiteSpace(rot))
                Rotation = new Rotation(rot);
        }
    }
}
