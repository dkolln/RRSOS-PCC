using System.Diagnostics;

namespace RRSOS_PCC.Services
{
    public class PCLauncherService
    {
        private const string ProcessName = "Planet Crafter";
        private const string SteamLaunchUri = "steam://rungameid/1284190";

        public bool IsRunning()
        {
            return Process.GetProcessesByName(ProcessName).Any();
        }

        public void Launch()
        {
            if (IsRunning())
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
            }

            // Give Windows time to release the process handle
            await Task.Delay(750);

            // Double-check that it's actually gone
            while (IsRunning())
            {
                await Task.Delay(200);
            }

            // Launch fresh
            Launch();
        }

    }
}
