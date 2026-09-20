namespace RRSOS_PCC.Classes
{
    public enum PollPhase
    {
        /// <summary>No save seen yet; checking at the slow interval.</summary>
        Unpredicted,

        /// <summary>A save was just loaded; sleeping until shortly before the next one is due.</summary>
        Waiting,

        /// <summary>The next save is imminent; checking the file every second or so.</summary>
        Watching,

        /// <summary>The expected save did not arrive; back to the slow interval.</summary>
        Overdue
    }

    /// <summary>What to do next and what the UI should show.</summary>
    public readonly record struct PollPlan(TimeSpan Delay, PollPhase Phase, DateTime? ExpectedUtc);

    /// <summary>Snapshot of the poller's state for the status display (immutable, safe to share).</summary>
    public sealed record SaveSchedule(PollPhase Phase, DateTime? ExpectedUtc)
    {
        public static readonly SaveSchedule None = new(PollPhase.Unpredicted, null);
    }

    /// <summary>
    /// Predictive save polling. The game writes a save on a steady cadence (about once a
    /// minute), so instead of checking on a fixed timer we anchor to the save file's own
    /// last-write time, sleep until just before the next one is due, then check quickly
    /// until it lands.
    /// </summary>
    public static class SavePollPlanner
    {
        public static PollPlan Plan(DateTime nowUtc, DateTime? lastWriteUtc, TimeSpan? learnedInterval, SaveSettings cfg)
        {
            var slow = TimeSpan.FromSeconds(Math.Max(1, cfg.AutoRefreshIntervalSeconds));
            var fine = TimeSpan.FromMilliseconds(Math.Max(100, cfg.FinePollIntervalMs));

            if (lastWriteUtc is null)
                return new PollPlan(slow, PollPhase.Unpredicted, null);

            var interval = learnedInterval ?? TimeSpan.FromSeconds(Math.Max(1, cfg.ExpectedSaveIntervalSeconds));
            var expected = lastWriteUtc.Value + interval;
            var watchFrom = expected - TimeSpan.FromSeconds(Math.Max(0, cfg.EarlyPollLeadSeconds));
            var overdueAt = expected + TimeSpan.FromSeconds(Math.Max(0, cfg.OverdueGraceSeconds));

            if (nowUtc < watchFrom)
            {
                // Cap the sleep so a file timestamp far in the future (clock change, touched
                // file) can never park the poller for hours.
                var sleep = watchFrom - nowUtc;
                var cap = interval + interval;
                return new PollPlan(sleep < cap ? sleep : cap, PollPhase.Waiting, expected);
            }

            if (nowUtc <= overdueAt)
                return new PollPlan(fine, PollPhase.Watching, expected);

            return new PollPlan(slow, PollPhase.Overdue, expected);
        }
    }

    /// <summary>
    /// Learns the real gap between saves from the last few timestamps, so a changed autosave
    /// setting is picked up automatically. The median ignores the odd skipped or extra save.
    /// </summary>
    public sealed class SaveIntervalTracker
    {
        private const int Capacity = 5;
        private static readonly TimeSpan MinGap = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan MaxGap = TimeSpan.FromMinutes(30);

        private readonly object _lock = new();
        private readonly Queue<TimeSpan> _gaps = new();
        private string? _path;
        private DateTime? _lastWriteUtc;

        public DateTime? LastWriteUtc
        {
            get { lock (_lock) return _lastWriteUtc; }
        }

        public TimeSpan? Median
        {
            get
            {
                lock (_lock)
                {
                    if (_gaps.Count == 0)
                        return null;

                    var sorted = _gaps.OrderBy(g => g).ToArray();
                    return sorted[sorted.Length / 2];
                }
            }
        }

        public void Observe(string path, DateTime writeUtc)
        {
            lock (_lock)
            {
                if (!string.Equals(path, _path, StringComparison.OrdinalIgnoreCase))
                {
                    // A different save slot has its own rhythm.
                    _gaps.Clear();
                }
                else if (_lastWriteUtc is DateTime previous && writeUtc > previous)
                {
                    var gap = writeUtc - previous;

                    if (gap >= MinGap && gap <= MaxGap)
                    {
                        _gaps.Enqueue(gap);

                        while (_gaps.Count > Capacity)
                            _gaps.Dequeue();
                    }
                }

                _path = path;
                _lastWriteUtc = writeUtc;
            }
        }
    }
}
