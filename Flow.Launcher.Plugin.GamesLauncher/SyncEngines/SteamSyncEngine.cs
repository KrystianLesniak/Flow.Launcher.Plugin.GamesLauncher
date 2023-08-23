using Flow.Launcher.Plugin.GamesLauncher.Handlers;
using Flow.Launcher.Plugin.GamesLauncher.Models;
using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using NexusMods.Paths;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.GamesLauncher.SyncEngines
{
    internal class SteamSyncEngine : ISyncEngine
    {
        public string PlatformName => "Steam";


        private readonly SteamHandler handler = new(FileSystem.Shared, WindowsRegistry.Shared);
        private const string steamLaunchUri = "steam://launch/";

        private readonly GameHandler gameHandler;
        public SteamSyncEngine(GameHandler gameHandler)
        {
            this.gameHandler = gameHandler;
        }

        public async IAsyncEnumerable<Game> GetGames()
        {
            var result = handler.FindAllGames();
            //TODO: Error handling
            var games = result.Where(x => x.IsGame()).Select(x => x.AsGame());

            foreach (var game in games)
            {
                yield return MapSteamGameToGame(game);
            }

            await Task.CompletedTask;
        }

        private Game MapSteamGameToGame(SteamGame steamGame)
        {
            var uriString = steamLaunchUri + steamGame.AppId.ToString().Trim();

            return new Game(
                Title: steamGame.Name,
                RunTask: gameHandler.GetGameRunTaskByUri(PlatformName, steamGame.Name, uriString),
                IconPath: GetIconPath(steamGame),
                Platform: PlatformName,
                IconDelegate: null
                );
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
