using Flow.Launcher.Plugin.GamesLauncher.Models;
using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using NexusMods.Paths;
using System;
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
        private readonly IPublicAPI publicApi;
        private const string steamLaunchUri = "steam://launch/";
        public SteamSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
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
            return new Game(
                Title: steamGame.Name,
                RunTask: (context) =>
                {
                    var uriString = steamLaunchUri + steamGame.AppId.ToString().Trim();

                    publicApi.OpenAppUri(new Uri(uriString));

                    return ValueTask.FromResult(true);
                },
                IconPath: GetIconPath(steamGame),
                Platform: PlatformName
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
