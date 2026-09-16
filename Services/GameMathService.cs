using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using System.Numerics;

namespace RRSOS_PCC.Services
{
    public class GameMathService
    {
        public GameMathService()
        {
        }

        public double DistanceToPlayer(Vector2 obj, Player player)
        {
            return PCMath.GetDistance(obj, player.Position.Flat);
        }

        public Dictionary<string, float> GetTIPriority(Planet planet)
        {
            var values = new Dictionary<string, float>
            {
                { "  Oxygen", planet.OxygenPct },
                { "  Heat", planet.HeatPct },
                { "  Pressure", planet.PressurePct },
                { "  Plants", planet.PlantsPct },
                { "  Insects", planet.InsectsPct },
                { "  Animals", planet.AnimalsPct }
            };

            var active = values.Where(v => v.Value > 0).ToDictionary(v => v.Key, v => v.Value);

            float expected = 1f / active.Count;

            return active
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => PCMath.Deviation(kvp.Value, expected)
                )
                .OrderBy(kvp => kvp.Value)
                .ToDictionary(k => k.Key, v => v.Value);
        }

        public float GetDeviation(Planet planet, string key)
        {
            var dict = GetTIPriority(planet);
            return dict.TryGetValue(key, out var v) ? v : 0f;
        }



    }
}
