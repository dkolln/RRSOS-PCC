using RRSOS_PCC.Enums;

namespace RRSOS_PCC.Models
{
    public class Base
    {
        public Dictionary<WorldObjectCategory, Dictionary<string, int>> Inventory { get; } = new();

        public Base()
        {
            Pods = new List<Pod>();
            Position = new Position("0,0,0");
        }

        public string Name { get; set; }

        public long id => EntrancePod?.id ?? 0;

        public List<Pod> Pods { get; set; }

        // Stable Position object
        public Position Position { get; set; }

        public float Elevation => Position.Elevation;

        public BaseType Type { get; set; }

        // Pod with door -> computed during binding
        public Pod EntrancePod =>
            Pods.FirstOrDefault(p => p.Panels.Any(panel => panel.HasDoor));

        // Hydrate after pods are assigned
        public void Hydrate()
        {
            if (EntrancePod != null)
                Position = EntrancePod.Position;
        }

        public void AddToContents(WorldObject wo)
        {
            if (!Inventory.TryGetValue(wo.Category, out var items))
            {
                items = new Dictionary<string, int>();
                Inventory[wo.Category] = items;
            }

            if (!items.ContainsKey(wo.Name))
                items[wo.Name] = 0;

            items[wo.Name]++;
        }
    }
}
