using Flow.Launcher.Plugin;
using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using NexusMods.Paths;

namespace GamesLauncher.Platforms.SyncEngines
{
    internal class SteamSyncEngine : ISyncEngine
    {
        public string PlatformName => "Steam";


        private readonly SteamHandler handler = new(FileSystem.Shared, WindowsRegistry.Shared);
        private readonly IPublicAPI publicApi;

        public SteamSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async IAsyncEnumerable<Game> GetGames()
        {
            var result = handler.FindAllGames();
            var games = result.Where(x => x.IsGame()).Select(x => x.AsGame());

            foreach (var game in games)
            {
                yield return MapSteamGameToGame(game);
            }

            await Task.CompletedTask;
        }

        private Game MapSteamGameToGame(SteamGame steamGame)
        {
            return new Game(
                Title: steamGame.Name,
                RunTask: GetGameRunTask(steamGame.AppId.ToString()),
                IconPath: GetIconPath(steamGame),
                Platform: PlatformName,
                IconDelegate: null
                );
        }

        private Func<ActionContext, ValueTask<bool>> GetGameRunTask(string gameAppIdString)
        {
            var uriString = $"steam://launch/{gameAppIdString}/Dialog";

            return (context) =>
            {
                publicApi.OpenAppUri(new Uri(uriString));

                return ValueTask.FromResult(true);
            };
        }

        private static string GetIconPath(SteamGame game)
        {
            var appIdString = game.AppId.ToString().Trim();
            var iconCachePath = Path.Combine(game.SteamPath.ToString(), "appcache", "librarycache", $"{appIdString}_icon.jpg");

            if (!File.Exists(iconCachePath))
                return Path.Combine("Icons", "steam.png");

            return iconCachePath;
        }
    }
}
