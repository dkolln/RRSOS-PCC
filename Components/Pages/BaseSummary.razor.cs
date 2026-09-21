using Microsoft.AspNetCore.Components;
using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Components.Pages
{
    /// <summary>Lists the bases, nearest first. Clicking one pins it in the contents panel.</summary>
    public partial class BaseSummary : ComponentBase
    {
        private const int ShowNumberBases = 100;

        [Parameter]
        public BaseSummaryViewModel? Data { get; set; }

        /// <summary>The base the contents panel is showing.</summary>
        [Parameter]
        public long? SelectedId { get; set; }

        /// <summary>The base picked by hand, if any. Null while the nearest base is being followed.</summary>
        [Parameter]
        public long? PinnedId { get; set; }

        [Parameter]
        public EventCallback<long> OnSelect { get; set; }

        private IEnumerable<BaseViewModel> DisplayList =>
            Data?.Bases.Take(ShowNumberBases) ?? Enumerable.Empty<BaseViewModel>();
    }
}
