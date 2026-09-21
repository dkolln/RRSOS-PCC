using Microsoft.AspNetCore.Components;
using RRSOS_PCC.ViewModels;
using RRSOS_PCC.Classes;

namespace RRSOS_PCC.Components.Pages
{
    public partial class VehicleSummary : ComponentBase
    {
        [Parameter]
        public VehicleViewModel? Data { get; set; }

        /// <summary>The player, so the vehicle can be shown relative to them on the small map.</summary>
        [Parameter]
        public PlayerViewModel? Player { get; set; }

        // UI-ready grouped lists
        protected List<(string Name, int Count)> LeftColumn { get; private set; } = new();
        protected List<(string Name, int Count)> RightColumn { get; private set; } = new();

        protected List<(string Name, int Count)> ModuleLeftColumn { get; private set; } = new();
        protected List<(string Name, int Count)> ModuleRightColumn { get; private set; } = new();

        protected override void OnParametersSet()
        {
            if (Data is null)
                return;

            // --- Trunk grouping ---
            var (left, right) = GroupingHelper.GroupAndSplit(
                Data.Trunk,
                x => x.DisplayName
            );

            LeftColumn = left;
            RightColumn = right;

            // --- Module grouping ---
            var (mLeft, mRight) = GroupingHelper.GroupAndSplit(
                Data.Modules,
                x => x.DisplayName
            );

            ModuleLeftColumn = mLeft;
            ModuleRightColumn = mRight;
        }
    }
}
