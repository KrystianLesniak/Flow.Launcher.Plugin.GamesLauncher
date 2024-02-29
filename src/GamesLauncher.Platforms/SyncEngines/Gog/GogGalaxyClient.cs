using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall;

namespace GamesLauncher.Platforms.SyncEngines.Gog
{
    internal class GogGalaxyClient
    {
        internal string IconPath { get; private set; } = string.Empty;
        internal string ExePath { get; private set; } = string.Empty;

        internal static async Task<GogGalaxyClient?> GetIfInstalled()
        {
            var gogUninstall = (await ControlPanelUninstall.GetPrograms()).Where(x => x.DisplayName == "GOG GALAXY").FirstOrDefault();

            if (gogUninstall is null)
                return null;

            var clientExeLocation = Path.Combine(gogUninstall.InstallLocation, "GalaxyClient.exe");

            if (!File.Exists(clientExeLocation))
                return null;

            return new GogGalaxyClient
            {
                ExePath = clientExeLocation,
                IconPath = Path.Combine(gogUninstall.InstallLocation, "Icons", "default.ico")
            };
        }
    }
}
