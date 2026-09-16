namespace RRSOS_PCC.Models
{
    public class UserSettings
    {
        public int DefaultBaseDistance { get; set; }
        public int AutoRefreshIntervalSeconds { get; set; }

        public string SelectedSaveFile { get; set; } = "Standard-1.json";
    }

}
