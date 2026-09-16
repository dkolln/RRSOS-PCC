using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using RRSOS_PCC.Services;

namespace RRSOS_PCC.ViewModels
{
    public class PlanetSummaryViewModel
    {
        public string PlanetId { get; }
        public float TotalTI { get; }

        public List<BreakdownRow> Breakdown { get; }

        public PlanetSummaryViewModel(Planet planet, GameMathService math)
        {
            PlanetId = planet.planetId;
            TotalTI = planet.TotalTI;

            Breakdown = new()
            {
                new BreakdownRow("Oxygen",   planet.Oxygen,   planet.OxygenPct,   math.GetDeviation(planet, "Oxygen")),
                new BreakdownRow("Heat",     planet.Heat,     planet.HeatPct,     math.GetDeviation(planet, "Heat")),
                new BreakdownRow("Pressure", planet.Pressure, planet.PressurePct, math.GetDeviation(planet, "Pressure")),
                new BreakdownRow("Plants",   planet.Plants,   planet.PlantsPct,   math.GetDeviation(planet, "Plants")),
                new BreakdownRow("Insects",  planet.Insects,  planet.InsectsPct,  math.GetDeviation(planet, "Insects")),
                new BreakdownRow("Animals",  planet.Animals,  planet.AnimalsPct,  math.GetDeviation(planet, "Animals"))
            };

            Breakdown = Breakdown
                .Where(r => r.Value > 0)
                .OrderBy(r => r.Deviation)
                .ToList();
        }

        public record BreakdownRow(string Name, double Value, double Percent, double Deviation);
    }
}
