namespace RRSOS_PCC.Classes
{
    /// <summary>
    /// Save-polling behaviour (bound from the "SaveSettings" section). Folder locations are
    /// handled by <see cref="PathResolver"/>. Every value has a sensible default.
    /// </summary>
    public class SaveSettings
    {
        /// <summary>Slow poll used when no save has been seen yet, or the next save is overdue.</summary>
        public int AutoRefreshIntervalSeconds { get; set; } = 10;

        /// <summary>Initial guess for how often the game writes a save, until we learn the real gap.</summary>
        public int ExpectedSaveIntervalSeconds { get; set; } = 60;

        /// <summary>Stop sleeping and start checking this long before the next save is expected.</summary>
        public int EarlyPollLeadSeconds { get; set; } = 10;

        /// <summary>How often to check the file while a save is imminent.</summary>
        public int FinePollIntervalMs { get; set; } = 1000;

        /// <summary>How long past the expected time before falling back to the slow poll.</summary>
        public int OverdueGraceSeconds { get; set; } = 20;
    }
}
