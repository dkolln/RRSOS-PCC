using System.Diagnostics;

namespace RRSOS_PCC.Services
{
    public class PCLauncherService
    {
        private const string ProcessName = "Planet Crafter";
        private const string SteamLaunchUri = "steam://rungameid/1284190";

        // Scanning every process on the machine is not free; callers ask often.
        private static readonly TimeSpan CacheFor = TimeSpan.FromSeconds(2);

        private readonly object _lock = new();
        private DateTime _checkedAtUtc = DateTime.MinValue;
        private bool _running;

        public bool IsRunning()
        {
            lock (_lock)
            {
                if (DateTime.UtcNow - _checkedAtUtc < CacheFor)
                    return _running;

                return CheckNow();
            }
        }

        /// <summary>Bypasses the cache; use when the answer must be current (e.g. waiting for exit).</summary>
        public bool CheckNow()
        {
            lock (_lock)
            {
                var processes = Process.GetProcessesByName(ProcessName);
                try
                {
                    _running = processes.Length > 0;
                }
                finally
                {
                    foreach (var p in processes)
                        p.Dispose();
                }

                _checkedAtUtc = DateTime.UtcNow;
                return _running;
            }
        }

        public void Launch()
        {
            if (CheckNow())
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = SteamLaunchUri,
                UseShellExecute = true
            });
        }

        public async Task ForceRelaunch()
        {
            // Kill existing instances
            foreach (var proc in Process.GetProcessesByName(ProcessName))
            {
                try { proc.Kill(); }
                catch { /* ignore */ }
                finally { proc.Dispose(); }
            }

            // Give Windows time to release the process handle
            await Task.Delay(750);

            // Double-check that it's actually gone
            while (CheckNow())
            {
                await Task.Delay(200);
            }

            // Launch fresh
            Launch();
        }

    }
}
