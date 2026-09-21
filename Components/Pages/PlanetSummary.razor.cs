using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Classes;
using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Components.Pages
{
    public partial class PlanetSummary : ComponentBase
    {
        [Parameter]
        public PlanetViewModel? Data { get; set; }

        [Parameter]
        public PowerSummaryViewModel? Power { get; set; }

        private static string? StatusName(TiStatus status) => status switch
        {
            TiStatus.Good => "good",
            TiStatus.Warn => "warn",
            TiStatus.Crit => "crit",
            _ => null
        };

        // Signed whole points in the middle of the dial: -21, +3, or a plain 0 when exactly fair.
        private static string Signed(float deviation)
        {
            var rounded = (int)Math.Round(deviation);
            return rounded == 0 ? "0" : (rounded < 0 ? "−" : "+") + Math.Abs(rounded);
        }

        // The ends of the dial, e.g. "−50" and "+50".
        private static string Scale(string sign) => sign + TiBalance.ScalePoints.ToString("0");

        // Everything that is not on the card itself: shown only on hover.
        private static string Tooltip(TiCategory category)
        {
            if (category.Status == TiStatus.Idle)
                return $"{category.Name}: not started yet";

            var direction = category.Deviation < 0 ? "below" : "above";

            var text = $"{category.Name}: {category.Share:0.0}% of the total. " +
                       $"{Math.Abs(category.Deviation):0.0} points {direction} an equal share " +
                       $"({category.FairShare:0.0}%, split across {category.ActiveCount} categories).";

            return category.Balanced ? text + " The planet is balanced, so nothing is flagged." : text;
        }
    }
}
