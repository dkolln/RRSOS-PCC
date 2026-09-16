using RRSOS_PCC.Classes;
using System.Numerics;

namespace RRSOS_PCC.Models
{
    public class Wreck
    {
        // Raw fields from save file
        public int owner { get; set; }
        public int planet { get; set; }
        public int index { get; set; }
        public int seed { get; set; }
        public string pos { get; set; } = "";
        public string rot { get; set; } = "";
        public bool wrecksWOGenerated { get; set; }
        public string woIdsGenerated { get; set; } = "";
        public string woIdsDropped { get; set; } = "";
        public int version { get; set; }

        // Parsed value objects
        public Position Position { get; private set; }
        public Rotation Rotation { get; private set; }

        public Wreck()
        {
            Position = new Position("0,0,0");
            Rotation = new Rotation("0,0,0,1");
        }

        // Call this after deserialization
        public void Hydrate()
        {
            Position.Update(pos);
            Rotation.Update(rot);
        }

        public List<int> GeneratedIds =>
            woIdsGenerated.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(int.Parse)
                          .ToList();

        public List<int> DroppedIds =>
            woIdsDropped.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
    }

}