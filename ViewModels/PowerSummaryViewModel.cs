namespace RRSOS_PCC.ViewModels
{
    /// <summary>One kind of machine and how much power all of them together make or use.</summary>
    public sealed record PowerLine(string GId, string Name, int Count, decimal KwEach, string? Icon = null)
    {
        public decimal KwTotal => Count * KwEach;
    }

    public sealed record PowerSummaryViewModel(
        IReadOnlyList<PowerLine> Generation,
        IReadOnlyList<PowerLine> Consumption,
        IReadOnlyList<PowerLine> Unrated)
    {
        // Load thresholds for the dial: the share of capacity in use where it turns yellow, then red.
        public const float LoadWarnAt = 0.75f;
        public const float LoadCritAt = 0.90f;

        public static readonly PowerSummaryViewModel Empty = new(Array.Empty<PowerLine>(), Array.Empty<PowerLine>(), Array.Empty<PowerLine>());

        /// <summary>The most power the placed generators can supply.</summary>
        public decimal Capacity => Generation.Sum(l => l.KwTotal);

        /// <summary>What the machines with a known usage rate draw. Machines without a rate are not in this figure.</summary>
        public decimal Required => Consumption.Sum(l => l.KwTotal);

        /// <summary>Capacity left over; negative means the machines want more than the generators make.</summary>
        public decimal Spare => Capacity - Required;

        /// <summary>Usage rates exist, so <see cref="Required"/> means something.</summary>
        public bool HasUsageData => Consumption.Count > 0;

        public bool HasAnyMachines => Generation.Count > 0 || Consumption.Count > 0 || Unrated.Count > 0;

        /// <summary>Required as a share of capacity. Above 1 means overloaded; with no capacity, any usage is a full overload.</summary>
        public float Load => Capacity > 0
            ? (float)(Required / Capacity)
            : Required > 0 ? 1f : 0f;
    }
}
