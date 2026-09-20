using Microsoft.AspNetCore.Components;
using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Components.Pages
{
    /// <summary>Lists the bases, nearest first. Which one the contents panel shows is decided by Home.</summary>
    public partial class BaseSummary : ComponentBase
    {
        private const int ShowNumberBases = 100;

        [Parameter]
        public BaseSummaryViewModel? Data { get; set; }

        private IEnumerable<BaseViewModel> DisplayList =>
            Data?.Bases.Take(ShowNumberBases) ?? Enumerable.Empty<BaseViewModel>();
    }
}
