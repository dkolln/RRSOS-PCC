using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using System.Numerics;

namespace RRSOS_PCC.Models
{
    public class Player
    {
        // Raw fields from save file
        public long id { get; set; }
        public string? name { get; set; }

        public int? inventoryId { get; set; }
        public int? equipmentId { get; set; }

        public string? playerPosition { get; set; }
        public string? playerRotation { get; set; }

        public double? playerGaugeOxygen { get; set; }
        public double? playerGaugeThirst { get; set; }
        public double? playerGaugeHealth { get; set; }
        public double? playerGaugeToxic { get; set; }

        public bool? host { get; set; }
        public int? planet { get; set; }

        // ---------------------------------------------------------
        // Value Objects (stable)
        // ---------------------------------------------------------

        public Position Position { get; private set; } = new Position("0,0,0");
        public Rotation Rotation { get; private set; } = new Rotation("0,0,0,1");

        // Hydrate after deserialization
        public void Hydrate()
        {
            if (!string.IsNullOrWhiteSpace(playerPosition))
                Position.Update(playerPosition);

            if (!string.IsNullOrWhiteSpace(playerRotation))
                Rotation.Update(playerRotation);
        }

        public bool IsHost => host == true;

        // Planet enum mapping
        public PlanetName PlanetName =>
            planet.HasValue && Enum.IsDefined(typeof(PlanetName), planet.Value)
                ? (PlanetName)planet.Value
                : PlanetName.Unknown;

        public Backpack Backpack { get; set; } = new();

        public Equipment Equipment { get; set; } = new();
    }
}
