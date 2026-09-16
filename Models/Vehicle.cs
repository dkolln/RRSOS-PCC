using RRSOS_PCC.Classes;
using System.Numerics;
using System.Drawing;

namespace RRSOS_PCC.Models
{
    public class Vehicle
    {
        // Raw fields from save file
        public int id { get; set; }
        public string gId { get; set; } = string.Empty;
        public int liId { get; set; }
        public string siIds { get; set; }
        public string pos { get; set; } = string.Empty;
        public string rot { get; set; } = string.Empty;
        public int planet { get; set; }

        // Stable value objects
        public Position Position { get; private set; } = new Position("0,0,0");
        public Rotation Rotation { get; private set; } = new Rotation("0,0,0,1");

        public List<WorldObject> TrunkItems { get; set; } = new();
        public List<WorldObject> ModuleItems { get; set; } = new();

        // Hydrate after deserialization
        public void Hydrate()
        {
            if (!string.IsNullOrWhiteSpace(pos))
                Position.Update(pos);

            if (!string.IsNullOrWhiteSpace(rot))
                Rotation.Update(rot);
        }
    }
}
