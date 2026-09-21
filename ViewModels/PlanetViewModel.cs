using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class PlanetViewModel
    {
        private readonly Planet _planet;

        public PlanetViewModel(Planet planet)
        {
            _planet = planet;

            // Fixed order, so each dial is always in the same place: the three main ones on top,
            // the life ones below.
            Categories = TiBalance.Evaluate(new[]
            {
                ("Oxygen", planet.Oxygen),
                ("Heat", planet.Heat),
                ("Pressure", planet.Pressure),
                ("Plants", planet.Plants),
                ("Insects", planet.Insects),
                ("Animals", planet.Animals)
            });
        }

        public string PlanetId => _planet.planetId;

        public float TotalTI => _planet.TotalTI;

        /// <summary>The six terraformation categories with their balance, in display order.</summary>
        public IReadOnlyList<TiCategory> Categories { get; }
    }
}
