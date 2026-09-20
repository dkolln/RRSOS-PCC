using RRSOS_PCC.Models;
using RRSOS_PCC.ViewModels;
using System.Text.RegularExpressions;

namespace RRSOS_PCC.Services
{
    /// <summary>
    /// Works out how much power the placed machines make and use.
    ///
    /// A save file stores no power figures, only which machines exist and where, so each
    /// machine type's rate comes from its PowerKw entry in worldobjectdata.json: positive
    /// generates, negative uses. Machines are counted by gId. Only records with a position
    /// count: a machine cannot be stored, and a save can hold position-less leftover records
    /// that are not running. The game's power-consumption world setting scales what machines use.
    ///
    /// Generation is exact for every generator that has a rate. Usage is only as complete as
    /// the data: a consumer with no PowerKw is simply not counted.
    /// </summary>
    public class PowerService
    {
        private static readonly Regex GeneratorGId = new(@"^EnergyGenerator\d+$", RegexOptions.Compiled);

        private readonly WorldObjectClassifierService _objects;

        public PowerService(WorldObjectClassifierService objects)
        {
            _objects = objects;
        }

        /// <summary>
        /// The gId of every record that has a position. The save loader files machines into
        /// separate lists (anything with storage slots is a container, extractors and
        /// collectors have their own), so all of them have to be counted.
        /// </summary>
        private static IEnumerable<string> PlacedGIds(SaveState state)
        {
            return state.WorldObjects.Where(o => o.pos != null).Select(o => o.gId)
                .Concat(state.Containers.Where(o => o.pos != null).Select(o => o.gId))
                .Concat(state.OreExtractors.Where(o => o.pos != null).Select(o => o.gId))
                .Concat(state.WaterCollectors.Where(o => o.pos != null).Select(o => o.gId))
                .Concat(state.AlgaeGenerators.Where(o => o.pos != null).Select(o => o.gId))
                .Concat(state.WaterLifeGenerators.Where(o => o.pos != null).Select(o => o.gId))
                .Concat(state.Ecosystems.Where(o => o.pos != null).Select(o => o.gId))
                .Where(gId => !string.IsNullOrEmpty(gId));
        }

        public PowerSummaryViewModel Summarize(SaveState? state)
        {
            if (state == null)
                return PowerSummaryViewModel.Empty;

            // A missing or zero setting means the game default.
            var usageScale = state.SaveInfo.modifierPowerConsumption > 0
                ? (decimal)state.SaveInfo.modifierPowerConsumption
                : 1m;

            var generation = new List<PowerLine>();
            var consumption = new List<PowerLine>();
            var unrated = new List<PowerLine>();

            var placed = PlacedGIds(state).GroupBy(gId => gId);

            foreach (var group in placed)
            {
                var gId = group.Key;
                var count = group.Count();
                var known = _objects.TryGetExactDefinition(gId, out var definition);
                var name = known ? $"{definition.Name} {definition.Tier}".Trim() : gId;

                if (known && definition.PowerKw is decimal kw)
                {
                    if (kw >= 0)
                        generation.Add(new PowerLine(gId, name, count, kw));
                    else
                        consumption.Add(new PowerLine(gId, name, count, -kw * usageScale));
                }
                else if (GeneratorGId.IsMatch(gId))
                {
                    // We can see it is a generator but do not know its output, so the capacity would be short.
                    unrated.Add(new PowerLine(gId, name, count, 0m));
                }
            }

            return new PowerSummaryViewModel(
                generation.OrderByDescending(l => l.KwTotal).ToList(),
                consumption.OrderByDescending(l => l.KwTotal).ToList(),
                unrated.OrderBy(l => l.GId).ToList());
        }
    }
}
