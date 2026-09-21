using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class BaseSummaryViewModel
    {
        public List<BaseViewModel> Bases { get; set; } = new();

        public int ScanRadius { get; set; } = 100;

        /// <summary>Where the player stands (raw world X, Z) and which way they face, for the base map.</summary>
        public System.Numerics.Vector2 PlayerFlat { get; }

        public float PlayerHeading { get; }

        public BaseSummaryViewModel(List<Base> bases, Player player)
        {
            if (bases == null || player == null) return;

            PlayerFlat = player.Position.Flat;
            PlayerHeading = player.Rotation.HeadingDegrees;

            // Map raw data to ViewModels and calculate distances in one pass
            Bases = bases
                .Select(b =>
                {
                    // Resolve the name here or inside the BaseViewModel constructor
                    string name = b.Name ?? b.EntrancePod?.gId ?? "Unknown Facility";

                    return new BaseViewModel(b, player.Position);
                })
                .OrderBy(vm => vm.Distance) // Closest first
                .ToList();
        }
    }
}
