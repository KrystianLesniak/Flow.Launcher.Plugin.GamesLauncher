﻿using Flow.Launcher.Plugin;
using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using GamesLauncher.Platforms.SyncEngines.Common.Interfaces;
using NexusMods.Paths;

namespace GamesLauncher.Platforms.SyncEngines.Steam
{
    internal class SteamSyncEngine : ISyncEngine
    {
        public string PlatformName => "Steam";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();


        private readonly SteamHandler handler = new(FileSystem.Shared, WindowsRegistry.Shared);
        private readonly IPublicAPI publicApi;

        public SteamSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {
            var result = handler.FindAllGames();
            var games = result.Where(x => x.IsGame()).Select(x => x.AsGame());
            var syncedGames = new List<Game>();

            foreach (var game in games)
            {
                syncedGames.Add(MapSteamGameToGame(game));
            }

            SynchronizedGames = syncedGames;
            await Task.CompletedTask;
        }

        private Game MapSteamGameToGame(SteamGame steamGame)
        {
            return new Game(
                title: steamGame.Name,
                runTask: GetGameRunTask(steamGame.AppId.ToString()),
                iconPath: GetIconPath(steamGame),
                platform: PlatformName,
                iconDelegate: null,
                uninstallAction: new(GetSteamDeleteTask(steamGame.AppId.ToString()))
                );
        }

        private Func<Task> GetGameRunTask(string gameAppIdString)
        {
            var uriString = $"steam://launch/{gameAppIdString}/Dialog";

            return async () =>
            {
                publicApi.OpenAppUri(new Uri(uriString));
                await Task.CompletedTask;
            };
        }

        private Func<Task> GetSteamDeleteTask(string gameAppIdString)
        {
            return async () =>
            {
                publicApi.OpenAppUri(new Uri($"steam://uninstall/{gameAppIdString}"));

                for (int i = 0; i < 12; i++)
                {
                    var installedGames = handler.FindAllGames().Where(x => x.IsGame()).Select(x => x.AsGame());

                    if (installedGames.Any(x => x.AppId.ToString() == gameAppIdString) == false)
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                await SynchronizeGames();
            };
        }

        private static string GetIconPath(SteamGame game)
        {
            var appIdString = game.AppId.ToString().Trim();
            var iconCachePath = Path.Combine(game.SteamPath.ToString(), "appcache", "librarycache", appIdString);

            if(!Directory.Exists(iconCachePath))
                return Path.Combine("Icons", "steam.png");

            var files = Directory
                .GetFiles(iconCachePath, "*.jpg", SearchOption.TopDirectoryOnly)
                .Where(file => {
                    string fileName = Path.GetFileName(file);
                    return !fileName.StartsWith("header", StringComparison.OrdinalIgnoreCase) &&
                           !fileName.StartsWith("library", StringComparison.OrdinalIgnoreCase) &&
                           !fileName.StartsWith("logo", StringComparison.OrdinalIgnoreCase);
                });

            return files.FirstOrDefault() ?? Path.Combine("Icons", "steam.png");
        }
    }
}
