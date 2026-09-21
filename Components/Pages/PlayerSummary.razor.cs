using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Models;
using RRSOS_PCC.Classes;
using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Components.Pages
{
    public partial class PlayerSummary : ComponentBase
    {
        [Parameter]
        public PlayerViewModel? Data { get; set; }

        /// <summary>The bases, for the small map around the player. Null shows the plain position map instead.</summary>
        [Parameter]
        public BaseSummaryViewModel? Bases { get; set; }

        /// <summary>The base currently shown on the Base tab.</summary>
        [Parameter]
        public long? SelectedBaseId { get; set; }

        /// <summary>Raised with a base's id when it is clicked on the small map.</summary>
        [Parameter]
        public EventCallback<long> OnBaseSelect { get; set; }

        public List<(string Name, int Count)> LeftColumn { get; set; } = new();
        public List<(string Name, int Count)> RightColumn { get; set; } = new();

        public List<(string Name, int Count)> EquipmentLeftColumn { get; set; } = new();
        public List<(string Name, int Count)> EquipmentRightColumn { get; set; } = new();

        protected override void OnParametersSet()
        {
            if (Data is null)
                return;

            var items = Data.Player.Backpack?.Items?
                .Where(i => i != null && i.gId != null)
                .ToList() ?? new();

            var equipmentItems = Data.Player.Equipment?.Items?
                .Where(i => i != null && i.gId != null)
                .ToList() ?? new();

            // Group by raw name only
            (LeftColumn, RightColumn) =
                GroupingHelper.GroupAndSplit(
                    items,
                    i => i.Name!
                );

            // Group by raw name only
            (EquipmentLeftColumn, EquipmentRightColumn) =
                GroupingHelper.GroupAndSplit(
                    equipmentItems,
                    i => i.Name!
                );

            LeftColumn = LeftColumn
                .Select(x => (Name: (x.Name), Count: x.Count))
                .ToList();

            RightColumn = RightColumn
                .Select(x => (Name: x.Name, Count: x.Count))
                .ToList();

            EquipmentLeftColumn = EquipmentLeftColumn
                .Select(x => (Name: x.Name, Count: x.Count))
                .ToList();

            EquipmentRightColumn = EquipmentRightColumn
                .Select(x => (Name: x.Name, Count: x.Count))
                .ToList();

        }

        public void RefreshSummary()
        {
            if (Data is null)
                return;

            (LeftColumn, RightColumn) =
                GroupingHelper.GroupAndSplit(
                    Data.Player.Backpack.Items,
                    i => i.gId
                );

            StateHasChanged();
        }

    }
}
