using Microsoft.Extensions.Options;
using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace RRSOS_PCC.Services;

/// <summary>
/// Owns the current parsed save. A background loop watches the selected save file and
/// reloads it when it changes; subscribers to <see cref="OnChange"/> are told exactly once
/// per real change (or when the load-error status changes), never on an idle tick.
///
/// The game writes a save on a steady cadence, so the loop is predictive: after each load it
/// sleeps until shortly before the next save is due, then checks the file every second until
/// it lands (see <see cref="SavePollPlanner"/>).
/// </summary>
public class SaveService : IDisposable
{
    // A save the game is still writing can fail to parse; retry a few times, then wait for the file to change.
    private const int MaxRetriesPerFile = 5;

    private static readonly Regex GidPattern = new("\"gId\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);

    private readonly BaseNamingService _naming;
    private readonly WorldObjectClassifierService _objectService;
    private readonly SaveSettings _cfg;

    // Only one load at a time, no matter who asks (timer loop, layout, save-slot dropdown).
    private readonly SemaphoreSlim _loadGate = new(1, 1);

    // Interrupts the poll loop's sleep (slot switched, etc.)
    private readonly SemaphoreSlim _wake = new(0, 1);

    private readonly object _loopLock = new();
    private CancellationTokenSource? _loopCts;

    private readonly SaveIntervalTracker _intervals = new();
    private SaveSignature? _loadedSignature;
    private SaveSignature? _failedSignature;
    private int _failedAttempts;

    private volatile SaveState? _currentState;
    private volatile string? _lastLoadError;
    private volatile SaveSchedule _schedule = SaveSchedule.None;
    private long _lastLoadedAtTicks;

    public SaveState? CurrentState => _currentState;

    /// <summary>Set while the newest save could not be read; cleared by the next successful load.</summary>
    public string? LastLoadError => _lastLoadError;

    /// <summary>Where the poller is in its wait-for-next-save cycle (for the status display).</summary>
    public SaveSchedule Schedule => _schedule;

    public DateTime? LastLoadedAtUtc
    {
        get
        {
            var ticks = Interlocked.Read(ref _lastLoadedAtTicks);
            return ticks == 0 ? null : new DateTime(ticks, DateTimeKind.Utc);
        }
    }

    public UserSettings Settings { get; private set; }

    public bool AutoRefreshEnabled
    {
        get { lock (_loopLock) return _loopCts != null; }
    }

    public event Func<Task>? OnChange;

    public SaveService(IOptions<SaveSettings> options, BaseNamingService naming, WorldObjectClassifierService objectService)
    {
        _cfg = options.Value;
        _naming = naming;
        _objectService = objectService;

        Settings = LoadOrCreateUserSettings();

        Settings.SelectedSaveFile = ResolveSelectedSave(Settings.SelectedSaveFile);
        PathResolver.SelectedSaveFile = Settings.SelectedSaveFile;
    }

    // ------------------------------------------------------------------
    // User settings
    // ------------------------------------------------------------------

    private static UserSettings LoadOrCreateUserSettings()
    {
        var path = PathResolver.UserSettingsPath;

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var root = JsonSerializer.Deserialize<Dictionary<string, UserSettings>>(json);

                if (root != null && root.TryGetValue("Settings", out var settings))
                    return settings;

                Console.Error.WriteLine("[SaveService] usersettings.json is missing the 'Settings' key, recreating defaults.");
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"[SaveService] usersettings.json is malformed, recreating defaults: {ex.Message}");
            }
        }

        var defaults = new UserSettings();
        WriteUserSettings(defaults);
        return defaults;
    }

    private static void WriteUserSettings(UserSettings settings)
    {
        var root = new Dictionary<string, UserSettings> { ["Settings"] = settings };
        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });

        var path = PathResolver.UserSettingsPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
    }

    public void SaveUserSettings() => WriteUserSettings(Settings);

    /// <summary>Keep the saved choice if that file exists, otherwise fall back to the newest save.</summary>
    private static string ResolveSelectedSave(string? preferred)
    {
        if (!string.IsNullOrWhiteSpace(preferred) && File.Exists(Path.Combine(PathResolver.SavePath, preferred)))
            return preferred;

        return PathResolver.NewestSaveFile() ?? preferred ?? "";
    }

    public string SelectedSaveFile
    {
        get => Settings.SelectedSaveFile;
        set
        {
            if (string.Equals(Settings.SelectedSaveFile, value, StringComparison.OrdinalIgnoreCase))
                return;

            Settings.SelectedSaveFile = value;
            PathResolver.SelectedSaveFile = value;
            SaveUserSettings();

            // Don't wait out a long sleep before looking at the new slot.
            WakePoller();
        }
    }

    // ------------------------------------------------------------------
    // Loading
    // ------------------------------------------------------------------

    private enum LoadOutcome { Unchanged, Loaded, ErrorChanged }

    /// <summary>
    /// Loads the selected save if it changed since the last successful load. Safe to call from
    /// anywhere, any number of times: concurrent calls are serialised and an unchanged file is a no-op.
    /// </summary>
    /// <returns>True if a new state was loaded.</returns>
    public async Task<bool> Load(bool force = false)
    {
        LoadOutcome outcome;

        await _loadGate.WaitAsync();
        try
        {
            // Parsing takes tens of milliseconds; keep it off the caller's (UI) thread.
            outcome = await Task.Run(() => LoadCore(force));
        }
        finally
        {
            _loadGate.Release();
        }

        if (outcome != LoadOutcome.Unchanged)
            await NotifyStateChanged();

        return outcome == LoadOutcome.Loaded;
    }

    private LoadOutcome LoadCore(bool force)
    {
        var path = PathResolver.FullSavePath;

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return SetError($"Save file not found: {(string.IsNullOrEmpty(path) ? "(none selected)" : path)}");

        var info = new FileInfo(path);
        var signature = new SaveSignature(path, info.LastWriteTimeUtc, info.Length);

        if (!force)
        {
            if (signature == _loadedSignature)
                return LoadOutcome.Unchanged;

            // Gave up on this exact version of the file; wait for the game to write a new one.
            if (signature == _failedSignature && _failedAttempts >= MaxRetriesPerFile)
                return LoadOutcome.Unchanged;
        }

        try
        {
            var raw = ReadShared(path);

            var state = new SaveProcessor(this, _naming, _objectService).Process(raw);
            state.SavePath = path;
            state.LoadedAt = DateTime.Now;

            // The signature is from *before* the read, so if the game rewrote the file while we
            // were reading, the next poll sees a different signature and reloads.
            _currentState = state;
            _loadedSignature = signature;
            _failedSignature = null;
            _failedAttempts = 0;
            _intervals.Observe(path, signature.LastWriteUtc);

            Interlocked.Exchange(ref _lastLoadedAtTicks, DateTime.UtcNow.Ticks);
            _lastLoadError = null;

            return LoadOutcome.Loaded;
        }
        catch (Exception ex)
        {
            // Most often the game caught mid-autosave (locked or truncated file). Keep the last
            // good state and retry; the loop backs off between attempts.
            _failedAttempts = signature == _failedSignature ? _failedAttempts + 1 : 1;
            _failedSignature = signature;

            Console.Error.WriteLine($"[SaveService] Load failed for '{path}' (attempt {_failedAttempts}): {ex}");

            return SetError(_failedAttempts >= MaxRetriesPerFile
                ? $"Could not read save, waiting for the next one: {ex.Message}"
                : $"Save file busy or unreadable, retrying: {ex.Message}");
        }
    }

    private LoadOutcome SetError(string message)
    {
        var changed = _lastLoadError != message;
        _lastLoadError = message;
        return changed ? LoadOutcome.ErrorChanged : LoadOutcome.Unchanged;
    }

    /// <summary>Reads while the game may still hold the file open for writing.</summary>
    private static string ReadShared(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private readonly record struct SaveSignature(string Path, DateTime LastWriteUtc, long Length);

    // ------------------------------------------------------------------
    // Change notification
    // ------------------------------------------------------------------

    private async Task NotifyStateChanged()
    {
        var handlers = OnChange?.GetInvocationList();
        if (handlers == null)
            return;

        // Await every subscriber (a plain `await OnChange.Invoke()` only awaits the last one)
        // and let one faulty subscriber fail alone instead of blocking the rest.
        var pending = new List<Task>(handlers.Length);

        foreach (var handler in handlers.Cast<Func<Task>>())
        {
            try { pending.Add(handler()); }
            catch (Exception ex) { Console.Error.WriteLine($"[SaveService] OnChange subscriber failed: {ex.Message}"); }
        }

        foreach (var task in pending)
        {
            try { await task; }
            catch (Exception ex) { Console.Error.WriteLine($"[SaveService] OnChange subscriber failed: {ex.Message}"); }
        }
    }

    // ------------------------------------------------------------------
    // Predictive polling
    // ------------------------------------------------------------------

    /// <summary>Starts watching for new saves (idempotent) and makes sure the current one is loaded.</summary>
    public Task StartAutoRefresh()
    {
        lock (_loopLock)
        {
            if (_loopCts == null)
            {
                _loopCts = new CancellationTokenSource();
                var token = _loopCts.Token;
                _ = Task.Run(() => RunLoopAsync(token));
            }
        }

        return Load();
    }

    public Task StopAutoRefresh()
    {
        lock (_loopLock)
        {
            _loopCts?.Cancel();
            _loopCts?.Dispose();
            _loopCts = null;
        }

        _schedule = SaveSchedule.None;
        return Task.CompletedTask;
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Load();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SaveService] Poll failed: {ex}");
            }

            var delay = NextDelay();

            try
            {
                await _wake.WaitAsync(delay, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private TimeSpan NextDelay()
    {
        var plan = SavePollPlanner.Plan(DateTime.UtcNow, _intervals.LastWriteUtc, _intervals.Median, _cfg);
        _schedule = new SaveSchedule(plan.Phase, plan.ExpectedUtc);

        // A read that failed is probably the game mid-write: try again soon.
        if (_failedSignature != null && _failedAttempts < MaxRetriesPerFile)
            return TimeSpan.FromSeconds(Math.Min(_failedAttempts * 2, 10));

        return plan.Delay;
    }

    private void WakePoller()
    {
        try { _wake.Release(); }
        catch (SemaphoreFullException) { /* already signalled */ }
    }

    public void Dispose()
    {
        StopAutoRefresh();
    }

    // ------------------------------------------------------------------
    // Parsing hooks (called by SaveProcessor)
    // ------------------------------------------------------------------

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

    // ------------------------------------------------------------------
    // Utilities
    // ------------------------------------------------------------------

    /// <summary>gIds present in the loaded save that worldobjectdata.json does not know about.</summary>
    public List<string> GetMissingGids()
    {
        var state = _currentState;
        if (state == null)
            return new List<string>();

        var gids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in state.RawJson)
        {
            foreach (Match match in GidPattern.Matches(record))
            {
                var gid = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(gid))
                    gids.Add(gid);
            }
        }

        return gids
            .Where(gid => !_objectService.IsKnownGid(gid))
            .OrderBy(gid => gid)
            .ToList();
    }

    public void OpenSaveFolder()
    {
        var folder = PathResolver.SavePath;

        if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
    }
}
