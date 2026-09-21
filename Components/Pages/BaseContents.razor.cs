using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using RRSOS_PCC.ViewModels;
using System.Numerics;

namespace RRSOS_PCC.Components.Pages
{
    public partial class BaseContents : ComponentBase
    {
        [Parameter]
        public BaseViewModel? SelectedBase { get; set; }

        /// <summary>True when the base was picked by hand, false when it is simply the nearest one.</summary>
        [Parameter]
        public bool IsPinned { get; set; }

        /// <summary>Raised when the user drops a pinned base and goes back to following the nearest.</summary>
        [Parameter]
        public EventCallback OnFollowNearest { get; set; }

        private ProcessedBaseContents? Processed;

        private GroupMode CurrentGroup = GroupMode.Category;

        private string SearchText = "";

        // Home re-renders this panel with a fresh SelectedBase after every new save.
        protected override void OnParametersSet() => Rebuild();

        private void Rebuild()
        {
            Processed = SelectedBase is null
                ? null
                : BaseContentsBuilder.Build(SaveSvc.CurrentState, SelectedBase.Id, CurrentGroup);
        }

        private void SetGroup(GroupMode mode)
        {
            CurrentGroup = mode;
            Rebuild();
        }

        private string GroupTitle => CurrentGroup switch
        {
            GroupMode.Name => "Stored, by name",
            GroupMode.Type => "Stored, by type",
            _ => "Stored, by category"
        };

        // Matches what the card shows (the item's name) as well as its raw id.
        private IEnumerable<WorldObject> FilteredWorldObjects =>
            string.IsNullOrWhiteSpace(SearchText) || SaveSvc.CurrentState == null
                ? Enumerable.Empty<WorldObject>()
                : SaveSvc.CurrentState.WorldObjects
                    .Where(wo => wo.gId.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                 || (wo.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        private IEnumerable<ItemSearchResult> SearchResults
        {
            get
            {
                var player = SaveSvc.CurrentState?.Player;

                return FilteredWorldObjects
                    .Select(wo =>
                    {
                        var baseObj = wo.OwningBase;

                        return new ItemSearchResult
                        {
                            ItemName = string.IsNullOrWhiteSpace(wo.Name) ? wo.gId : wo.Name,
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
                            Direction = player != null
                                ? PCMath.GetCompassDirection(player.Position.Flat, wo.Position.Flat)
                                : ""
                        };
                    })
                    .OrderBy(x => x.Distance)
                    .ThenBy(y => y.ItemName);
            }
        }

        private IEnumerable<GroupedSearchResult> GroupedResults =>
            SearchResults
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
