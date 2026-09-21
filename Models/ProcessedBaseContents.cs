using RRSOS_PCC.Classes;

namespace RRSOS_PCC.Models
{
    /// <summary>A counted list of names, also split into two columns for display.</summary>
    public class ItemList
    {
        public List<(string Name, int Count)> All { get; }
        public List<(string Name, int Count)> Left { get; }
        public List<(string Name, int Count)> Right { get; }

        public bool Any => All.Count > 0;

        public ItemList(List<(string Name, int Count)> items)
        {
            All = items;
            (Left, Right) = GroupingHelper.SplitOnly(items);
        }

        public static ItemList Empty => new(new());
    }

    public class ProcessedBaseContents
    {
        // Items inside containers, grouped by whatever the card is set to group by
        public ItemList Stored { get; set; } = ItemList.Empty;

        // Crops in growers that are fully grown
        public ItemList ReadyToHarvest { get; set; } = ItemList.Empty;

        // Loose items on the ground within the base
        public ItemList Boneyard { get; set; } = ItemList.Empty;
    }
}
