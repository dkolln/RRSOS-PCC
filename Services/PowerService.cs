using RRSOS_PCC.Enums;
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
    /// A generator's PowerKw is its base output. Nearby Machine Optimizers holding Energy fuses
    /// multiply it (see <see cref="GeneratorMultipliers"/>), so generators of one type can
    /// appear on several lines with different outputs.
    ///
    /// Generation is exact for every generator that has a rate. Usage is only as complete as
    /// the data: a consumer with no PowerKw is simply not counted, and optimizer effects on
    /// what machines draw are not modelled (the rates are what the game showed for the
    /// optimizer set-up they were read under).
    /// </summary>
    public class PowerService
    {
        private static readonly Regex GeneratorGId = new(@"^EnergyGenerator\d+$", RegexOptions.Compiled);

        // Machine Optimizer rules, confirmed by the user against a late-game world.
        // Each fuse is worth 150% and the boost is the sum of the fuses, with no extra +100%
        // base: three fuses make a generator 4.5x, three plus one from a second optimizer 6x.
        private const decimal FuseBonus = 1.5m;

        // How far an optimizer reaches. Roughly two floor panels; a panel is 6 m in the saves
        // measured, and a generator 9.0 m away was definitely served, so it is at least 9.
        private const float OptimizerReachMeters = 12f;

        // How many machines each optimizer can serve, nearest first. Fuse slots are not
        // listed because the fuses actually inside are read from the save.
        private static readonly Dictionary<string, int> OptimizerMachineLimit = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Optimizer1"] = 5,
            ["Optimizer2"] = 8
        };

        private const string EnergyFusePrefix = "FuseEnergy";

        private readonly WorldObjectClassifierService _objects;

        public PowerService(WorldObjectClassifierService objects)
        {
            _objects = objects;
        }

        /// <summary>
        /// The id and gId of every record that has a position. The save loader files machines
        /// into separate lists (anything with storage slots is a container, extractors and
        /// collectors have their own), so all of them have to be counted.
        /// </summary>
        private static IEnumerable<(long Id, string GId)> PlacedRecords(SaveState state)
        {
            return state.WorldObjects.Where(o => o.pos != null).Select(o => (o.id, o.gId))
                .Concat(state.Containers.Where(o => o.pos != null).Select(o => (o.id, o.gId)))
                .Concat(state.OreExtractors.Where(o => o.pos != null).Select(o => (o.id, o.gId)))
                .Concat(state.WaterCollectors.Where(o => o.pos != null).Select(o => (o.id, o.gId)))
                .Concat(state.AlgaeGenerators.Where(o => o.pos != null).Select(o => (o.id, o.gId)))
                .Concat(state.WaterLifeGenerators.Where(o => o.pos != null).Select(o => (o.id, o.gId)))
                .Concat(state.Ecosystems.Where(o => o.pos != null).Select(o => (o.id, o.gId)))
                .Where(r => !string.IsNullOrEmpty(r.gId));
        }

        private bool IsGenerator(string gId) =>
            _objects.TryGetExactDefinition(gId, out var definition) && definition.Type == WorldObjectType.PowerGenerator;

        /// <summary>
        /// Output multiplier for each generator that an Energy-fuse optimizer serves, by object id.
        /// Every optimizer serves the generators nearest to it that are within reach, up to its
        /// machine limit, and adds 150% per Energy fuse it holds; several optimizers stack.
        /// A generator no optimizer serves is absent and runs at 1x. The save does not record
        /// which machines an optimizer serves, so this is worked out from positions.
        /// </summary>
        private Dictionary<long, decimal> GeneratorMultipliers(SaveState state)
        {
            var multipliers = new Dictionary<long, decimal>();

            var generators = state.WorldObjects
                .Where(o => o.pos != null && IsGenerator(o.gId))
                .ToList();

            if (generators.Count == 0)
                return multipliers;

            foreach (var optimizer in state.Containers)
            {
                if (optimizer.pos == null || !OptimizerMachineLimit.TryGetValue(optimizer.gId, out var limit))
                    continue;

                var fuses = optimizer.PrimaryItems.Count(i => i.gId.StartsWith(EnergyFusePrefix, StringComparison.OrdinalIgnoreCase));
                if (fuses == 0)
                    continue;

                var served = generators
                    .Select(g => (Generator: g, Distance: optimizer.Position.DistanceTo(g.Position)))
                    .Where(x => x.Distance <= OptimizerReachMeters)
                    .OrderBy(x => x.Distance)
                    .Take(limit);

                foreach (var (generator, _) in served)
                    multipliers[generator.id] = multipliers.GetValueOrDefault(generator.id) + fuses * FuseBonus;
            }

            return multipliers;
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

            var multipliers = GeneratorMultipliers(state);

            // Group by gId and boost, so five generators at 6x and three at 4.5x are two lines.
            var placed = PlacedRecords(state)
                .Select(r => (r.GId, Boost: multipliers.TryGetValue(r.Id, out var m) ? m : 1m))
                .GroupBy(r => r);

            foreach (var group in placed)
            {
                var (gId, boost) = group.Key;
                var count = group.Count();
                var known = _objects.TryGetExactDefinition(gId, out var definition);
                var name = known ? $"{definition.Name} {definition.Tier}".Trim() : gId;

                if (known && definition.PowerKw is decimal kw)
                {
                    if (kw >= 0)
                    {
                        var label = boost == 1m ? name : $"{name} ({boost:0.##}×)";
                        generation.Add(new PowerLine(gId, label, count, kw * boost, definition.Icon));
                    }
                    else
                    {
                        consumption.Add(new PowerLine(gId, name, count, -kw * usageScale));
                    }
                }
                else if (GeneratorGId.IsMatch(gId))
                {
                    // We can see it is a generator but do not know its output, so the capacity would be short.
                    unrated.Add(new PowerLine(gId, name, count, 0m, known ? definition.Icon : null));
                }
            }

            // Lines that share a gId and a rate (e.g. from a duplicate group) are already merged by the grouping.
            return new PowerSummaryViewModel(
                generation.OrderByDescending(l => l.KwTotal).ToList(),
                consumption.OrderByDescending(l => l.KwTotal).ToList(),
                unrated.OrderBy(l => l.GId).ToList());
        }
    }
}
