namespace RRSOS_PCC.Classes
{
    public class SaveSettings
    {
        public string SavePath { get; set; } = "";
        public string SaveFile { get; set; } = "";
        public string AssetPath { get; set; } = "";
        public string BlueprintFile { get; set; } = "";
        public string ResourceFile { get; set; } = "";
        public string NameResolverFile { get; set; } = "";
        public string BaseNamesFile { get; set; } = "";
        public string OutpostNamesFile { get; set; } = "";

        public int AutoRefreshIntervalSeconds { get; set; } = 1;

    }

}
