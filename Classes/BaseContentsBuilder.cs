using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;

namespace RRSOS_PCC.Classes
{
    /// <summary>
    /// Works out what one base holds, for the Base Contents card. Nothing here touches the UI,
    /// so the card only has to draw what it is handed.
    /// </summary>
    public static class BaseContentsBuilder
    {
        private const int FullyGrown = 100;

        public static ProcessedBaseContents Build(SaveState? state, long baseId, GroupMode mode)
        {
            var baseObj = state?.Bases.FirstOrDefault(b => b.id == baseId);
            if (state == null || baseObj == null)
                return new ProcessedBaseContents();

            // Everything that belongs to this base, without the machines and building parts.
            var objects = state.WorldObjects
                .Where(wo => wo.OwningBase?.id == baseObj.id
                             && wo.Category != WorldObjectCategory.Machine
                             && wo.Category != WorldObjectCategory.BasePart)
                .ToList();

            var stored = objects
                .Where(wo => wo.Owner.Type == WorldObjectOwnerType.Container)
                .GroupBy(wo => mode switch
                {
                    GroupMode.Name => wo.Name,
                    GroupMode.Type => wo.Type.ToString(),
                    _ => wo.Category.ToString()
                })
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderBy(x => x.Name)
                .ToList();

            var boneyard = objects
                .Where(wo => wo.Owner.Type == WorldObjectOwnerType.World)
                .GroupBy(wo => wo.gId)
                .Select(g => (Name: g.First().Name, Count: g.Count()))
                .OrderBy(x => x.Name)
                .ToList();

            return new ProcessedBaseContents
            {
                Stored = new ItemList(stored),
                ReadyToHarvest = new ItemList(ReadyCrops(state, baseObj)),
                Boneyard = new ItemList(boneyard)
            };
        }

        // Crops sitting in this base's growers that have finished growing.
        private static List<(string Name, int Count)> ReadyCrops(SaveState state, Base baseObj)
        {
            var ready = new List<string>();

            foreach (var c in state.Containers.Where(c => c.OwningBase?.id == baseObj.id))
            {
                if (c.SecondaryItems == null || c.OwningBase != baseObj)
                    continue;

                if (!c.gId.Contains("VegetableGrower") && !c.gId.Contains("Farm1"))
                    continue;

                ready.AddRange(c.SecondaryItems.Where(i => i.grwth == FullyGrown).Select(i => i.Name));
            }

            return ready
                .GroupBy(name => name)
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
