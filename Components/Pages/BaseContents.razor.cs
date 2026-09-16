using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using RRSOS_PCC.ViewModels;
using System.Numerics;

namespace RRSOS_PCC.Components.Pages
{
    public partial class BaseContents : ComponentBase, IDisposable
    {
        [Parameter]
        public BaseViewModel? SelectedBase { get; set; }

        public ProcessedBaseContents? Processed { get; set; }

        public List<(string Name, int Count)> GrowerItems { get; set; } = new();
        public List<(string Name, int Count)> GrowerItemsView { get; set; } = new();

        private Func<Task>? _onChangeHandler;

        private GroupMode CurrentGroup = GroupMode.Category;

        protected override void OnInitialized()
        {
            _onChangeHandler = RefreshContentsAsync;
            SaveSvc.OnChange += _onChangeHandler;
        }

        private async Task RefreshContentsAsync()
        {
            if (SelectedBase != null)
            {
                Processed = BuildContentsForBase(SelectedBase);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected override void OnParametersSet()
        {
            if (SelectedBase != null)
                Processed = BuildContentsForBase(SelectedBase);
        }

        private ProcessedBaseContents BuildContentsForBase(BaseViewModel baseVM)
        {
            var state = SaveSvc.CurrentState;
            if (state == null) return new ProcessedBaseContents();

            var baseObj = state.Bases.FirstOrDefault(b => b.id == baseVM.Id);
            if (baseObj == null) return new ProcessedBaseContents();

            List<(string Name, int Count)> items;

            // All world objects that belong to this base
            var allRelevantObjects = state.WorldObjects
                .Where(wo => wo.OwningBase?.id == baseObj.id)
                .ToList();

            allRelevantObjects.RemoveAll(x => x.Category == WorldObjectCategory.Machine);
            allRelevantObjects.RemoveAll(x => x.Category == WorldObjectCategory.BasePart);

            switch (CurrentGroup)
            {
                case GroupMode.Name:
                    items = allRelevantObjects
                        .Where(wo => wo.Owner.Type == WorldObjectOwnerType.Container)
                        .GroupBy(wo => wo.Name)
                        .Select(g => (Name: g.Key, Count: g.Count()))
                        .OrderBy(x => x.Name)
                        .ToList();
                    break;

                case GroupMode.Type:
                    items = allRelevantObjects
                        .Where(wo => wo.Owner.Type == WorldObjectOwnerType.Container)
                        .GroupBy(wo => wo.Type.ToString())   // or wo.Type if you have one
                        .Select(g => (Name: g.Key, Count: g.Count()))
                        .OrderBy(x => x.Name)
                        .ToList();
                    break;

                default: // Category
                    items = allRelevantObjects
                        .Where(wo => wo.Owner.Type == WorldObjectOwnerType.Container)
                        .GroupBy(wo => wo.Category.ToString())
                        .Select(g => (Name: g.Key, Count: g.Count()))
                        .OrderBy(x => x.Name)
                        .ToList();
                    break;
            }

            var containers = state.Containers
                .Where(c => c.OwningBase?.id == baseObj.id)
                .ToList();

            // -------------------------------
            // SAFE GROWER ITEM COLLECTION
            // -------------------------------
            var growerList = new List<(string Name, int Count)>();

            foreach (var c in containers)
            {
                if (c.SecondaryItems != null &&
                    (c.gId.Contains("VegetableGrower") || c.gId.Contains("Farm1")) &&
                    c.OwningBase == baseObj)
                {
                    foreach (var item in c.SecondaryItems)
                    {
                        if (item.grwth == 100)
                            growerList.Add((item.Name, 1));
                    }
                }
            }

            growerList = growerList
                .GroupBy(x => x.Name)
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderBy(x => x.Name)
                .ToList();

            GrowerItems = growerList;
            GrowerItemsView = growerList.ToList();

            // -------------------------------
            // MAIN ITEM GROUPING
            // -------------------------------
            var (left, right) = GroupingHelper.SplitOnly(items);

            var categorized = new List<CategoryGroupViewModel>
            {
                new CategoryGroupViewModel
                {
                    CategoryName = CurrentGroup.ToString(),
                    Items = items,
                    Left = left,
                    Right = right
                }
            };

            // Boneyard (loose items)
            var boneyard = allRelevantObjects
                .Where(wo => wo.Owner.Type == WorldObjectOwnerType.World)
                .GroupBy(wo => wo.gId)
                .Select(g => (Name: g.First().Name, Count: g.Count()))
                .OrderBy(x => x.Name)
                .ToList();

            var (bLeft, bRight) = GroupingHelper.SplitOnly(boneyard);

            return new ProcessedBaseContents
            {
                CategorizedItems = categorized,
                Boneyard = boneyard,
                BoneyardLeft = bLeft,
                BoneyardRight = bRight
            };
        }

        private string SearchText = "";

        private IEnumerable<WorldObject> FilteredWorldObjects =>
            string.IsNullOrWhiteSpace(SearchText)
                ? Enumerable.Empty<WorldObject>()
                : SaveSvc.CurrentState.WorldObjects
                    .Where(wo => wo.gId.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        private IEnumerable<ItemSearchResult> SearchResults
        {
            get
            {
                var player = SaveSvc.CurrentState.Player;

                return FilteredWorldObjects
                    .Select(wo =>
                    {
                        var baseObj = wo.OwningBase;

                        return new ItemSearchResult
                        {
                            ItemName = wo.gId,
                            ItemID = wo.id,
                            BaseName = baseObj != null
                                ? $"{BaseNamingSvc.GetBaseName(baseObj)} ({baseObj.id})"
                                : "No Base",
                            BasePos = baseObj != null
                                ? $"{baseObj.Position.Flat.X:F0}, {baseObj.Position.Flat.Y:F0}"
                                : "—",
                            Distance = player != null
                                ? (int)Vector2.Distance(
                                    wo.Position.Flat,
                                    player.Position.Flat)
                                : 0,
                            Direction = PCMath.GetCompassDirection(player.Position.Flat, wo.Position.Flat)
                        };
                    })
                    .OrderBy(x => x.Distance)
                    .ThenBy(y => y.ItemName);
            }
        }

        private IEnumerable<GroupedSearchResult> GroupedResults
        {
            get
            {
                var raw = SearchResults; // your existing property

                return raw
                    .GroupBy(r => r.ItemName)
                    .Select(g => new GroupedSearchResult
                    {
                        ItemName = g.Key,
                        Locations = g
                            .GroupBy(r => r.BaseName)
                            .Select(loc => new LocationGroup
                            {
                                LocationName = loc.Key,
                                Count = loc.Count(),
                                NearestDistance = loc.Min(x => x.Distance),
                                Direction = loc
                                    .OrderBy(x => x.Distance)
                                    .First().Direction,
                                BasePos = loc.First().BasePos
                            })
                            .OrderBy(l => l.NearestDistance)
                            .ToList()
                    })
                    .OrderBy(g => g.ItemName)
                    .ToList();
            }
        }

        private void SetGroup(GroupMode mode)
        {
            CurrentGroup = mode;

            if (SelectedBase != null)
                Processed = BuildContentsForBase(SelectedBase);

            StateHasChanged();
        }



        public void Dispose()
        {
            if (_onChangeHandler != null)
                SaveSvc.OnChange -= _onChangeHandler;
        }
    }
}
