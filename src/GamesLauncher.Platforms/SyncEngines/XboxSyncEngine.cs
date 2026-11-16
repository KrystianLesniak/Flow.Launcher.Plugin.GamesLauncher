using Flow.Launcher.Plugin;
using GameFinder.Common;
using GameFinder.StoreHandlers.Xbox;
using GamesLauncher.Platforms.SyncEngines.Common;
using GamesLauncher.Platforms.SyncEngines.Common.Interfaces;
using Microsoft.WindowsAPICodePack.Shell;
using NexusMods.Paths;
using static Flow.Launcher.Plugin.Result;

namespace GamesLauncher.Platforms.SyncEngines
{
    internal class XboxSyncEngine : ISyncEngine
    {
        public string PlatformName => "Xbox";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();

        private readonly XboxHandler handler = new(FileSystem.Shared);
        private readonly Guid FODLERID_AppsFolder = new("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");

        private readonly IPublicAPI publicApi;

        public XboxSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {
            var games = handler.FindAllGames().Where(x => x.IsGame()).Select(x => x.AsGame()).ToList();
            var syncedGames = new List<Game>();

            List<ShellObject> apps = games.Count == 0 ? [] : KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder).ToList();

            foreach (var game in games)
            {
                var shellGame = apps.FirstOrDefault(x => x.ParsingName.StartsWith(game.Id.Value + "_") || x.ParsingName.StartsWith(game.Id.Value + "!"));

                if (shellGame != null)
                {
                    syncedGames.Add(MapShellObjectToGame(shellGame));
                }
            }

            SynchronizedGames = syncedGames;

            await Task.CompletedTask;
        }

        private Game MapShellObjectToGame(ShellObject shellGame)
        {
            var cmd = $"shell:appsFolder\\{shellGame.ParsingName}";

            var iconDelegate = GetIconDelegate(shellGame);

            return new Game(
                title: shellGame.Name,
                platform: PlatformName,
                runTask: GetGameRunTask(cmd),
                iconPath: iconDelegate is null ? Path.Combine("Icons", "xbox.png") : null,
                iconDelegate: iconDelegate,
                uninstallAction: GetXboxUninstallActionTask(shellGame)
                );
        }

        private Func<Task> GetGameRunTask(string cmd)
        {
            return () =>
            {
                publicApi.ShellRun(cmd, "explorer.exe");
                return Task.CompletedTask;
            };
        }

        private UninstallAction? GetXboxUninstallActionTask(ShellObject shellGame)
        {
            var parsingName = shellGame.ParsingName;

            if (shellGame.Properties.GetProperty("System.AppUserModel.PackageFullName").ValueAsObject is not string packageFullname)
                return null;

            return new UninstallAction(async () =>
            {
                publicApi.ShellRun($"Remove-AppxPackage -Package {packageFullname}", "powershell.exe");

                await EngineHelpers.WaitUntilGone(() =>
                    Task.FromResult(!KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder).Any(x => x.ParsingName == parsingName)));

                await SynchronizeGames();
            },
            "Uninstall",
            "Remove this Xbox game from your PC? Its files will be deleted.");
        }

        private static IconDelegate? GetIconDelegate(ShellObject shellGame)
        {
            var bitmapSource = shellGame.Thumbnail.LargeBitmapSource;

            if (bitmapSource == null || bitmapSource.CanFreeze == false)
                return null;

            bitmapSource.Freeze(); //This is needed. Otherwise FL throws exception for some users.

            return delegate ()
            {
                return bitmapSource;
            };
        }
    }
}
