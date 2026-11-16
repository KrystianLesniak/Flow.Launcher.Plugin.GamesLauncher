using Flow.Launcher.Plugin;
using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.GOG;
using GamesLauncher.Platforms.SyncEngines.Common;
using GamesLauncher.Platforms.SyncEngines.Common.Interfaces;
using NexusMods.Paths;

namespace GamesLauncher.Platforms.SyncEngines.Gog
{
    internal class GogSyncEngine : ISyncEngine
    {
        public string PlatformName => "GOG Galaxy";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();


        private readonly GOGHandler handler = new (WindowsRegistry.Shared, FileSystem.Shared);
        private readonly IPublicAPI publicApi;

        public GogSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {
            var gogClient = await GogGalaxyClient.GetIfInstalled();
            if (gogClient is null)
                return;

            var games = handler.FindAllGames().Where(x => x.IsGame()).Select(x => x.AsGame());
            var syncedGames = new List<Game>();

            foreach (var game in games)
            {
                syncedGames.Add(MapGogGameToGame(game, gogClient));
            }

            SynchronizedGames = syncedGames;
        }

        private Game MapGogGameToGame(GOGGame gogGame, GogGalaxyClient gogClient)
        {
            return new Game(
                title: gogGame.Name,
                runTask: GetGameRunTask(gogGame, gogClient),
                iconPath: GetIconPath(gogGame, gogClient),
                platform: PlatformName,
                iconDelegate: null,
                uninstallAction: GetGogUninstallTask(gogGame)
                );
        }

        private Func<Task> GetGameRunTask(GOGGame gogGame, GogGalaxyClient gogClient)
        {
            var argString = $"/command=runGame /gameId={gogGame.Id} /path=\"{gogGame.Path}\"";

            return () =>
            {
                publicApi.ShellRun(argString, gogClient.ExePath);
                return Task.CompletedTask;
            };
        }
        private static string GetIconPath(GOGGame gogGame, GogGalaxyClient gogClient)
        {
            var icoPath = Path.Combine(gogGame.Path.ToString(), $"goggame-{gogGame.Id}.ico");

            if (File.Exists(icoPath))
                return icoPath;
            else
                return gogClient.IconPath;
        }

        private UninstallAction? GetGogUninstallTask(GOGGame gogGame)
        {
            var uninstallExe = Path.Combine(gogGame.Path.ToString(), "unins000.exe");

            if (!File.Exists(uninstallExe))
                return null;

            return new UninstallAction(async () =>
            {
                publicApi.RunFile(uninstallExe);

                await EngineHelpers.WaitUntilGone(() =>
                    Task.FromResult(!handler.FindAllGames().Where(x => x.IsGame()).Select(x => x.AsGame()).Any(x => x.Id == gogGame.Id)));

                await SynchronizeGames();
            });
        }
    }
}
