using RRSOS_PCC.Classes;
using System.Text.Json;

namespace RRSOS_PCC.Models
{
    public class Planet
    {
        // Raw fields from save file
        public string planetId { get; set; }
        public float unitOxygenLevel { get; set; }
        public float unitHeatLevel { get; set; }
        public float unitPressureLevel { get; set; }
        public float unitPlantsLevel { get; set; }
        public float unitInsectsLevel { get; set; }
        public float unitAnimalsLevel { get; set; }
        public float unitPurificationLevel { get; set; }

        // ---------------------------------------------------------
        // Derived fields (clean, expressive, UI-friendly)
        // ---------------------------------------------------------

        public float Oxygen => unitOxygenLevel;
        public float Heat => unitHeatLevel;
        public float Pressure => unitPressureLevel;
        public float Plants => unitPlantsLevel;
        public float Insects => unitInsectsLevel;
        public float Animals => unitAnimalsLevel;
        public float Purification => unitPurificationLevel;

        // Total TI (Terraforming Index)
        public float TotalTI => Oxygen + Heat + Pressure + Plants + Insects + Animals;

        // TI breakdown percentages
        public float OxygenPct => PCMath.SafePct(Oxygen, TotalTI);
        public float HeatPct => PCMath.SafePct(Heat, TotalTI);
        public float PressurePct => PCMath.SafePct(Pressure, TotalTI);
        public float PlantsPct => PCMath.SafePct(Plants, TotalTI);
        public float InsectsPct => PCMath.SafePct(Insects, TotalTI);
        public float AnimalsPct => PCMath.SafePct(Animals, TotalTI);
    }


}
