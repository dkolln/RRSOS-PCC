using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class TiRow
    {
        public string Name { get; set; }
        public float Value { get; set; }
        public float Percent { get; set; }
    }

    public class PlanetViewModel
    {
        private readonly Planet _planet;

        public PlanetViewModel(Planet planet)
        {
            _planet = planet;
        }

        public string PlanetId => _planet.planetId;

        public float TotalTI => _planet.TotalTI;

        public IEnumerable<TiRow> Breakdown => new[]
        {
            new TiRow { Name = "Oxygen", Value = _planet.Oxygen, Percent = _planet.OxygenPct },
            new TiRow { Name = "Heat", Value = _planet.Heat, Percent = _planet.HeatPct },
            new TiRow { Name = "Pressure", Value = _planet.Pressure, Percent = _planet.PressurePct },
            new TiRow { Name = "Plants", Value = _planet.Plants, Percent = _planet.PlantsPct },
            new TiRow { Name = "Insects", Value = _planet.Insects, Percent = _planet.InsectsPct },
            new TiRow { Name = "Animals", Value = _planet.Animals, Percent = _planet.AnimalsPct }
        };
    }

}
