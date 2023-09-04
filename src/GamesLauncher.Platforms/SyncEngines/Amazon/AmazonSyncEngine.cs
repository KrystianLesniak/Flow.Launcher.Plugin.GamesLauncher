using Flow.Launcher.Plugin;
using GamesLauncher.Platforms.SyncEngines.Amazon.Readers;

namespace GamesLauncher.Platforms.SyncEngines.Amazon
{
    internal class AmazonSyncEngine : ISyncEngine
    {
        public string PlatformName => "Amazon Games";

        private readonly IPublicAPI publicApi;

        public AmazonSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async IAsyncEnumerable<Game> GetGames()
        {
            await using var installInfoReader = new InstallInfoReader();
            await using var productDetailsReader = new ProductDetailsReader();

            await foreach (var gameInstallInfo in installInfoReader.GetInstalledGamesId())
            {
                yield return new Game(
                    Title: gameInstallInfo.Title,
                    RunTask: GetGameRunTask(gameInstallInfo.Id),
                    IconPath: await productDetailsReader.GetIconUrlById(gameInstallInfo.Id),
                    IconDelegate: null,
                    Platform: PlatformName
                );
            }
        }


        private Func<ActionContext, ValueTask<bool>> GetGameRunTask(string gameAppIdString)
        {
            var uriString = "amazon-games://play/" + gameAppIdString;

            return (context) =>
            {
                publicApi.OpenAppUri(new Uri(uriString));

                return ValueTask.FromResult(true);
            };
        }
    }
}
