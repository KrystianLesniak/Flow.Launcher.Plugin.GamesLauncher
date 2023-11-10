using Flow.Launcher.Plugin;
using GamesLauncher.Platforms.SyncEngines.Amazon.Readers;
using System.Collections.Concurrent;

namespace GamesLauncher.Platforms.SyncEngines.Amazon
{
    internal class AmazonSyncEngine : ISyncEngine
    {
        public string PlatformName => "Amazon Games";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();


        private readonly IPublicAPI publicApi;

        public AmazonSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {
            await using var installInfoReader = new InstallInfoReader();
            await using var productDetailsReader = new ProductDetailsReader();

            var syncedGames = new ConcurrentBag<Game>();

            await foreach (var gameInstallInfo in installInfoReader.GetInstalledGamesId())
            {
                syncedGames.Add(new Game(
                    title: gameInstallInfo.Title,
                    runTask: GetGameRunTask(gameInstallInfo.Id),
                    iconPath: await productDetailsReader.GetIconUrlById(gameInstallInfo.Id),
                    iconDelegate: null,
                    platform: PlatformName
                ));
            }

            SynchronizedGames = syncedGames;
        }


        private Func<Task> GetGameRunTask(string gameAppIdString)
        {
            var uriString = "amazon-games://play/" + gameAppIdString;

            return () =>
            {
                publicApi.OpenAppUri(new Uri(uriString));

                return Task.CompletedTask;
            };
        }
    }
}
