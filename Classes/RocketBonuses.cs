using RRSOS_PCC.Models;

namespace RRSOS_PCC.Classes
{
    /// <summary>What the launched rockets do for one stat (Oxygen, Heat, ...).</summary>
    /// <param name="Stat">Same names as the planet card's dials, plus "Purification".</param>
    /// <param name="Tier1">Tier 1 rockets launched.</param>
    /// <param name="Tier2">Tier 2 rockets launched.</param>
    /// <param name="Percent">Sum of what every launched rocket grants, in percent (1000 = x10).</param>
    /// <param name="NextTier1Percent">What one more tier 1 rocket would grant.</param>
    public sealed record RocketBonus(string Stat, int Tier1, int Tier2, decimal Percent, decimal NextTier1Percent)
    {
        public int Rockets => Tier1 + Tier2;

        /// <summary>The factor applied to the base rate: 3000% is x30.</summary>
        public decimal Multiplier => Percent / 100m;

        /// <summary>How much one more tier 1 rocket would raise the total rate, in percent of today's rate.</summary>
        public decimal NextGainPercent => Percent > 0 ? NextTier1Percent / Percent * 100m : 0m;
    }

    /// <summary>
    /// Works out the bonus the launched rockets give each stat.
    ///
    /// The game keeps launched rockets in hidden "space" storage objects (SpaceMultiplierHeat and so
    /// on, parked out of the world at -500,-500,-500), one per stat. Every rocket in one of them
    /// adds its own percentage to that stat, and the total rate is
    /// <c>machines * output per machine * rockets * multiplier</c> (the wiki's formula), so the
    /// total grows in step with the count while each further rocket adds less than the one before.
    /// </summary>
    public static class RocketBonuses
    {
        private const string SpacePrefix = "SpaceMultiplier";

        // What one rocket grants, by stat: tier 1 and tier 2 (null = no such tier). From the game's wiki.
        private static readonly Dictionary<string, (decimal Tier1, decimal? Tier2)> PerRocket = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Heat"] = (1000m, 5000m),       // Asteroid Attraction
            ["Pressure"] = (1000m, 5000m),   // Magnetic Field Protection
            ["Plants"] = (1250m, 5000m),
            ["Oxygen"] = (1000m, 6000m),
            ["Insects"] = (1500m, 5000m),
            ["Animals"] = (1750m, 6750m),
            ["Purification"] = (1000m, null)
        };

        /// <summary>One entry per stat that has a space container in the save. Empty when nothing is launched.</summary>
        public static IReadOnlyDictionary<string, RocketBonus> Summarize(SaveState? state)
        {
            var result = new Dictionary<string, RocketBonus>(StringComparer.OrdinalIgnoreCase);

            if (state?.Containers == null)
                return result;

            foreach (var container in state.Containers)
            {
                if (container.gId == null || !container.gId.StartsWith(SpacePrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var stat = container.gId[SpacePrefix.Length..];

                if (!PerRocket.TryGetValue(stat, out var perRocket))
                    continue;

                // Tier 2 rockets have a trailing 2 in their id (RocketMap2, RocketInformations2, ...).
                var tier2 = container.PrimaryItems.Count(i => i.gId != null && i.gId.EndsWith('2'));
                var tier1 = container.PrimaryItems.Count - tier2;
                var percent = tier1 * perRocket.Tier1 + tier2 * (perRocket.Tier2 ?? 0m);

                result[stat] = new RocketBonus(stat, tier1, tier2, percent, perRocket.Tier1);
            }

            return result;
        }
    }
}
