using Flow.Launcher.Plugin;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models;
using GamesLauncher.Platforms.SyncEngines.Common.Interfaces;
using GamesLauncher.Platforms.SyncEngines.EaApp.Models;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace GamesLauncher.Platforms.SyncEngines.EaApp
{
    internal partial class EaAppSyncEngine : ISyncEngine
    {
        public string PlatformName => "EA app";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();

        private readonly string GameInstallerDataSubdirectory = Path.Combine("__Installer", "installerdata.xml");

        private readonly IPublicAPI publicApi;

        public EaAppSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {

            var eaAppUninstallPrograms = (await ControlPanelUninstall.GetPrograms())
                .Where(x => x.UninstallCommand != null && x.UninstallCommand.Contains("EAInstaller") && x.UninstallCommand.Contains("Cleanup.exe"));

            if (!eaAppUninstallPrograms.Any())
                return;

            var syncedGames = new List<Game>();

            foreach (var eaUninstallProgram in eaAppUninstallPrograms)
            {
                var installerDataXmlPath = Path.Combine(eaUninstallProgram.InstallLocation, GameInstallerDataSubdirectory);

                if (!File.Exists(installerDataXmlPath)) continue;

                var gameExePath = GetGameExePathFromInstallerData(eaUninstallProgram.InstallLocation, installerDataXmlPath) 
                                  ?? GetGameExePathFromUninstallProgram(eaUninstallProgram);

                if (gameExePath is null) continue;

                syncedGames.Add(new Game(
                    title: eaUninstallProgram.DisplayName,
                    platform: PlatformName,
                    runTask: async () =>
                    {
                        var directory = Path.GetDirectoryName(gameExePath);
                        var fileExe = Path.GetFileName(gameExePath);
                        publicApi.ShellRun($"cd /d \"{directory}\" && start \"\" \"{fileExe}\"");
                        await Task.CompletedTask;
                    },
                    iconPath: eaUninstallProgram.DisplayIconPath ?? Path.Combine("Icons", "ea.ico"),
                    uninstallAction: GetUninstallAction(eaUninstallProgram)
                ));
            }

            SynchronizedGames = syncedGames;
        }

        private static string? GetGameExePathFromInstallerData(string installLocation, string installerDataXmlPath)
        {
            var xmlSerializer = new XmlSerializer(typeof(InstallerDataXml));
            using var xmlReader = XmlReader.Create(installerDataXmlPath);

            if (!xmlSerializer.CanDeserialize(xmlReader)) return null;
            if (xmlSerializer.Deserialize(xmlReader) is not InstallerDataXml installerData) return null;

            var launcherData = installerData.Runtime?.Launchers.FirstOrDefault(x => x.Trial == false);

            if (launcherData is null) return null;                //TODO: Test how trials behaves

            var gameExe = RegistryKeyEaPaths().Replace(launcherData.FilePath, string.Empty);
            var gameExePath = Path.Combine(installLocation, gameExe);

            if(!File.Exists(gameExePath)) return null;

            return gameExePath;
        }

        private static string? GetGameExePathFromUninstallProgram(UninstallProgram eaUninstallProgram)
        {
            if (Path.GetExtension(eaUninstallProgram.DisplayIconPath) == ".exe")
                return eaUninstallProgram.DisplayIconPath;

            return null;
        }

        private UninstallAction? GetUninstallAction(UninstallProgram eaUninstallProgram)
        {
            var uninstallCommand = eaUninstallProgram.UninstallCommand;
            var subKeyName = eaUninstallProgram.SubKeyName;

            if (uninstallCommand == null) return null;

            return new UninstallAction(async () =>
            {
                publicApi.ShellRun(uninstallCommand);

                for (int i = 0; i < 12; i++)
                {
                    var programs = await ControlPanelUninstall.GetPrograms();

                    if (programs.Any(x => x.SubKeyName.Equals(subKeyName)) == false)
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                await SynchronizeGames();
            });
        }

        [GeneratedRegex("\\[.*?\\]")]
        private static partial Regex RegistryKeyEaPaths();
    }
}
