namespace RRSOS_PCC.Classes
{
    /// <summary>
    /// Single source of truth for every folder and file the app touches.
    ///
    /// Three locations:
    ///   SavePath     - the game's save folder (read-only for us). Auto-detected from
    ///                  %LOCALAPPDATA%\..\LocalLow\MijuGames\Planet Crafter unless overridden.
    ///   AssetPath    - static reference data shipped with the app (resources, name pools,
    ///                  worldobjectdata). Defaults to "&lt;content root&gt;\Assets".
    ///   UserDataPath - things the app writes (notebook, base names, user settings).
    ///                  Defaults to %LOCALAPPDATA%\RRSOS-PCC so the repo stays clean and a
    ///                  published build keeps its data.
    ///
    /// appsettings.json only needs SaveSettings:SavePath / AssetPath / UserDataPath when the
    /// defaults are wrong; leave them empty to auto-detect.
    /// </summary>
    public static class PathResolver
    {
        private const string AppFolderName = "RRSOS-PCC";

        public static string SavePath { get; private set; } = "";
        public static string AssetPath { get; private set; } = "";
        public static string UserDataPath { get; private set; } = "";

        public static string SelectedSaveFile { get; set; } = "";

        public static string FullSavePath =>
            string.IsNullOrWhiteSpace(SelectedSaveFile) ? "" : Path.Combine(SavePath, SelectedSaveFile);

        // Static reference data (Assets)
        public static string ResourcesPath => Path.Combine(AssetPath, "resources.json");
        public static string BaseNamesPath => Path.Combine(AssetPath, "basenames.json");
        public static string OutpostNamesPath => Path.Combine(AssetPath, "outpostnames.json");
        public static string WorldObjectDataPath => Path.Combine(AssetPath, "worldobjectdata.json");

        // User data (written by the app)
        public static string BaseDataPath => Path.Combine(UserDataPath, "basedata.json");
        public static string NotebookPath => Path.Combine(UserDataPath, "notebook.json");
        public static string UserSettingsPath => Path.Combine(UserDataPath, "usersettings.json");

        private static readonly string[] LegacyUserFiles = { "notebook.json", "basedata.json", "usersettings.json" };

        public static void Initialize(IConfiguration config, string contentRoot)
        {
            SavePath = ResolveFolder(config["SaveSettings:SavePath"], DetectSavePath(), "SavePath");
            AssetPath = ResolveFolder(config["SaveSettings:AssetPath"], Path.Combine(contentRoot, "Assets"), "AssetPath");

            var configuredUserData = config["SaveSettings:UserDataPath"];
            UserDataPath = string.IsNullOrWhiteSpace(configuredUserData)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName)
                : configuredUserData;

            Directory.CreateDirectory(UserDataPath);
            MigrateLegacyUserData();

            SelectedSaveFile = "";
        }

        /// <summary>The game keeps saves in LocalLow, which has no SpecialFolder entry.</summary>
        public static string DetectSavePath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appData = Path.GetDirectoryName(localAppData) ?? localAppData;
            return Path.Combine(appData, "LocalLow", "MijuGames", "Planet Crafter");
        }

        public static IReadOnlyList<string> ListSaveFiles()
        {
            try
            {
                if (!Directory.Exists(SavePath))
                    return Array.Empty<string>();

                return Directory.EnumerateFiles(SavePath, "*.json")
                    .Select(f => Path.GetFileName(f)!)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>Most recently written save, used when no (valid) save has been chosen yet.</summary>
        public static string? NewestSaveFile()
        {
            try
            {
                if (!Directory.Exists(SavePath))
                    return null;

                return Directory.EnumerateFiles(SavePath, "*.json")
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .Select(Path.GetFileName)
                    .FirstOrDefault();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        private static string ResolveFolder(string? configured, string fallback, string settingName)
        {
            if (string.IsNullOrWhiteSpace(configured))
                return fallback;

            if (Directory.Exists(configured))
                return configured;

            Console.Error.WriteLine(
                $"[PathResolver] SaveSettings:{settingName} '{configured}' does not exist; using '{fallback}' instead.");
            return fallback;
        }

        /// <summary>
        /// Earlier versions kept user data inside Assets. Copy it over once so notes,
        /// base names and settings survive the move. Never overwrites existing files.
        /// </summary>
        private static void MigrateLegacyUserData()
        {
            if (!Directory.Exists(AssetPath))
                return;

            foreach (var name in LegacyUserFiles)
            {
                var target = Path.Combine(UserDataPath, name);
                if (File.Exists(target))
                    continue;

                var legacy = Directory.EnumerateFiles(AssetPath)
                    .FirstOrDefault(f => string.Equals(Path.GetFileName(f), name, StringComparison.OrdinalIgnoreCase));

                if (legacy == null)
                    continue;

                File.Copy(legacy, target);
                Console.WriteLine($"[PathResolver] Migrated {name} from Assets to {UserDataPath}");
            }
        }
    }
}
