using RRSOS_PCC.Enums;
using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Models
{
    public record InventoryGroup(WorldObjectCategory Category, Dictionary<string, int> Items)
    {
        public string Header => Category.ToString().ToUpper();
        public IEnumerable<ResourceLine> Lines => Items
            .Select(i => new ResourceLine(i.Key, i.Value))
            .OrderByDescending(l => l.Count);
    }
}
