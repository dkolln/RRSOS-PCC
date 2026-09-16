using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using System.Numerics;

namespace RRSOS_PCC.Models
{
    public class WorldObject
    {
        // Raw fields from save file
        public long id { get; set; }
        public string gId { get; set; }
        public int? liId { get; set; }
        public string? pos { get; set; }
        public string? rot { get; set; }
        public int? planet { get; set; }
        public string? text { get; set; }
        public float? grwth { get; set; }

        // Value objects (stable)
        public Position Position { get; private set; } = new Position("0,0,0");
        public Rotation Rotation { get; private set; } = new Rotation("0,0,0,1");

        // Hydrated references
        public WorldObjectOwner Owner { get; set; } = new();

        public Base OwningBase { get; set; }

        public string Name { get; set; } = "";
        public string Tier { get; set; } = "";

        public WorldObjectType Type { get; set; } = WorldObjectType.Unknown;
        public WorldObjectCategory Category { get; set; } = WorldObjectCategory.Unknown;

        // Hydrate after deserialization
        public void Hydrate()
        {
            if (pos != null)
                Position.Update(pos);

            if (rot != null)
                Rotation.Update(rot);
        }
    }
}
