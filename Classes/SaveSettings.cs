namespace RRSOS_PCC.Classes
{
    /// <summary>
    /// Save-polling behaviour (bound from the "SaveSettings" section). Folder locations are
    /// handled by <see cref="PathResolver"/>. Every value has a sensible default.
    /// </summary>
    public class SaveSettings
    {
        /// <summary>How often the selected save is checked for a newer version. It is only reloaded if it changed.</summary>
        public int AutoRefreshIntervalSeconds { get; set; } = 10;
    }
}
