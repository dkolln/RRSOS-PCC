using Microsoft.Extensions.Options;
using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using System.Diagnostics;
using System.Runtime;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace RRSOS_PCC.Services;

public class SaveService
{
    private readonly IConfiguration _config;
    private string? _lastLoadedFile;

    public SaveState? CurrentState { get; private set; }
    
    private readonly BaseNamingService _naming;
    private readonly WorldObjectClassifierService _objectService;

    public static string UserSettingsPath => Path.Combine(PathResolver.AssetPath, "usersettings.json");
    public UserSettings Settings { get; private set; }

    private readonly System.Timers.Timer _autoRefreshTimer;
    private readonly int _refreshIntervalSeconds;

    public bool AutoRefreshEnabled { get; private set; } = false;

    public event Func<Task>? OnChange;

    private async Task NotifyStateChanged()
    {
        if (OnChange != null)
            await OnChange.Invoke();
    }


    public int RefreshIntervalSeconds => _refreshIntervalSeconds;

    public SaveService(IOptions<SaveSettings> options, BaseNamingService naming, WorldObjectClassifierService objectService)
    {
        var settings = options.Value;
        _naming = naming; // Store the naming service instance
        _objectService = objectService;

        _refreshIntervalSeconds = settings.AutoRefreshIntervalSeconds;

        _autoRefreshTimer = new System.Timers.Timer(
            TimeSpan.FromSeconds(_refreshIntervalSeconds).TotalMilliseconds);

        _autoRefreshTimer.Elapsed += (_, __) => AutoReload();
        _autoRefreshTimer.AutoReset = true;

        var json = File.ReadAllText(UserSettingsPath);
        var root = JsonSerializer.Deserialize<Dictionary<string, UserSettings>>(json);
        Settings = root["Settings"];

        PathResolver.SelectedSaveFile = Settings.SelectedSaveFile;
    }

    private async Task AutoReload()
    {
        if (!AutoRefreshEnabled)
            return;

        if (string.IsNullOrWhiteSpace(PathResolver.FullSavePath))
        {
            if(string.IsNullOrWhiteSpace(PathResolver.SecondarySavePath))
            {
                return;
            }
        }

        await Load();
        await NotifyStateChanged();
    }

    public string? LastLoadError { get; private set; }

    public async Task Load()
    {
        string sourceFile = PathResolver.FullSavePath;
        string secondaryfile = PathResolver.SecondarySavePath;

        string workingFile = PathResolver.WorkingFilePath;

        string path = File.Exists(sourceFile) ? sourceFile : secondaryfile;

        if (!File.Exists(path))
            return;

        var srcInfo = new FileInfo(path);
        var dstInfo = new FileInfo(workingFile);

        bool firstLoad = CurrentState == null;
        bool workingMissing = !dstInfo.Exists;
        bool sourceIsNewer = srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc;

        bool fileChanged = _lastLoadedFile != path;

        if (!(firstLoad || workingMissing || sourceIsNewer || fileChanged))
            return;

        await NotifyStateChanged();

        try
        {
            File.Copy(path, workingFile, overwrite: true);

            var raw = GetRawData(workingFile);

            var processor = new SaveProcessor(this, _naming, _objectService);
            var state = processor.Process(raw);

            state.SavePath = workingFile;
            state.LoadedAt = DateTime.Now;

            CurrentState = state;
            _lastLoadedFile = path;
            LastLoadError = null;

            // immersion delay only when a new file was actually loaded
            await Task.Delay(500);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // The game may still be mid-autosave (file locked or briefly truncated).
            // Keep the last good CurrentState and just retry on the next timer tick
            // instead of surfacing a broken/partial state or crashing the refresh loop.
            LastLoadError = $"Save file busy or unreadable, will retry: {ex.Message}";
            Console.Error.WriteLine($"[SaveService] Load failed for '{path}': {ex}");
        }
        catch (Exception ex)
        {
            LastLoadError = $"Unexpected error loading save: {ex.Message}";
            Console.Error.WriteLine($"[SaveService] Unexpected error loading '{path}': {ex}");
        }
        finally
        {
            await NotifyStateChanged();   // UI refreshes even if the load failed
        }
    }

    public List<string> ExtractGidsFromSave(string saveFilePath)
    {
        var text = File.ReadAllText(saveFilePath);

        // Regex: "gId":"SOMETHING"
        var matches = Regex.Matches(text, "\"gId\"\\s*:\\s*\"([^\"]+)\"");

        var gids = matches
            .Select(m => m.Groups[1].Value)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g)
            .ToList();

        return gids;
    }

    public List<string> GetMissingGids(string saveFilePath, string metadataJsonPath)
    {
        // Extract gIds from save file
        var saveGids = ExtractGidsFromSave(saveFilePath);

        // Load metadata dictionary
        var metadata = JsonSerializer.Deserialize<Dictionary<string, WorldObjectDataEntry>>(
            File.ReadAllText(metadataJsonPath));

        var metaGids = new HashSet<string>(metadata.Keys, StringComparer.OrdinalIgnoreCase);

        // Compute difference
        return saveGids
            .Where(gid => !metaGids.Contains(gid))
            .OrderBy(gid => gid)
            .ToList();
    }



    public async Task StartAutoRefresh()
    {
        if (!AutoRefreshEnabled)
        {
            AutoRefreshEnabled = true;
            _autoRefreshTimer.Start();
            await NotifyStateChanged();
        }
    }

    public async Task StopAutoRefresh()
    {
        if (AutoRefreshEnabled)
        {
            AutoRefreshEnabled = false;
            _autoRefreshTimer.Stop();
            await NotifyStateChanged();
        }
    }



    public string GetRawData(string path)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "rrs_check.json");
        File.Copy(path, tempPath, true);
        return File.ReadAllText(tempPath);
    }

    public List<string> ParseSave(string rawData)
    {
        return rawData.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void ProcessBlock(JsonBlock? block, SaveState state)
    {
        if (block == null)
        {
            return;
        }

        if (block.IsMultiEntry)
        {
            foreach (var entry in block.Entries)
                ProcessRouter.RouteLine(entry, state);
        }
        else
        {
            ProcessRouter.ProcessSingleton(block.Raw, state);
        }
    }

    public void BindBlocks(SaveState state, BaseNamingService naming, WorldObjectClassifierService objectService)
    {
        ProcessBinder.BindContainers(state, objectService);
        ProcessBinder.BindSigns(state);
        ProcessBinder.BindPods(state);
        ProcessBinder.BindBases(state, naming);
        ProcessBinder.BindWorldObjects(state, objectService);
        ProcessBinder.BindContainerOwner(state);
    }

    public bool RefreshSaves(Game game)
    {
        bool changed = false;

        var files = Directory.GetFiles(game.SaveFolderPath, "*.json");

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var lastUpdated = File.GetLastWriteTime(file);

            var existing = game.Saves.FirstOrDefault(s => s.FileName == fileName);

            if (existing == null)
            {
                // New file
                game.Saves.Add(new Save
                {
                    FileName = fileName,
                    LastUpdated = lastUpdated
                });

                changed = true;
            }
            else if (existing.LastUpdated != lastUpdated)
            {
                // Updated file
                existing.LastUpdated = lastUpdated;
                changed = true;
            }
        }

        return changed;
    }

    public void SaveUserSettings()
    {
        var root = new Dictionary<string, UserSettings>
        {
            ["Settings"] = Settings
        };

        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(UserSettingsPath, json);
    }

    public void OpenSaveFolder()
    {
        var folder = Path.GetDirectoryName(PathResolver.FullSavePath);

        if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
    }

    public string SelectedSaveFile
    {
        get => Settings.SelectedSaveFile;
        set
        {
            Settings.SelectedSaveFile = value;
            PathResolver.SelectedSaveFile = value;
            SaveUserSettings();
        }
    }






}