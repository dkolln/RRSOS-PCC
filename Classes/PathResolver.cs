using Microsoft.AspNetCore.StaticAssets;

namespace RRSOS_PCC.Classes
{
    public static class PathResolver
    {
        public static string SavePath { get; set; } = "";
        public static string SaveFile { get; set; } = "";
        public static string SecondarySaveFile { get; set; } = "";

        public static string AssetPath { get; set; } = "";
        public static string ResourceFile { get; set; } = "";
        public static string BlueprintFile { get; set; } = "";
        public static string BaseNamesFile { get; set; } = "";
        public static string OutpostNamesFile { get; set; } = "";
        public static string WorldObjectDataFile { get; set; } = "";
        public static string BaseDataFile { get; set; } = "";
        public static string NotebookDataFile { get; set; } = "";


        public static string SelectedSaveFile { get; set; } = "Standard-1.json";

        public static string FullSavePath => Path.Combine(SavePath, SelectedSaveFile);
        public static string SecondarySavePath => Path.Combine(AssetPath, SaveFile);

        public static string ResourcesPath => Path.Combine(AssetPath, ResourceFile);
        public static string WorkingFilePath => Path.Combine(AssetPath, SaveFile);
        public static string BlueprintPath => Path.Combine(AssetPath, BlueprintFile);
        public static string BaseNamesPath => Path.Combine(AssetPath, BaseNamesFile);
        public static string OutpostNamesPath => Path.Combine(AssetPath, OutpostNamesFile);
        public static string WorldObjectDataPath => Path.Combine(AssetPath, WorldObjectDataFile);
        public static string BaseDataPath => Path.Combine(AssetPath, BaseDataFile);
        public static string NotebookPath => Path.Combine(AssetPath, NotebookDataFile);
        
        public static void Initialize(IConfiguration config)
        {
            SavePath = config["SaveSettings:SavePath"] ?? "";
            SaveFile = config["SaveSettings:SaveFile"] ?? "";

            SecondarySaveFile = config["SaveSettings:SecondarySaveFile"] ?? "";
            AssetPath = config["SaveSettings:AssetPath"] ?? "";
            ResourceFile = config["SaveSettings:ResourceFile"] ?? "";
            BlueprintFile = config["SaveSettings:BlueprintFile"] ?? "";
            BaseNamesFile = config["SaveSettings:BaseNamesFile"] ?? "";
            OutpostNamesFile = config["SaveSettings:OutpostNamesFile"] ?? "";
            WorldObjectDataFile = config["SaveSettings:WorldObjectDataFile"] ?? "";
            BaseDataFile = config["SaveSettings:BaseDataFile"] ?? "";
            NotebookDataFile = config["SaveSettings:NotebookDataFile"] ?? "";

        }
    }
}
