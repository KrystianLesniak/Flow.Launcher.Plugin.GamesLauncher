using Flow.Launcher.Plugin;
using GamesLauncher.Common.Settings;
using GamesLauncher.Platforms.SyncEngines;
using GamesLauncher.Platforms.SyncEngines.EpicSyncEngine;
using System.Collections.Concurrent;

namespace GamesLauncher.Platforms
{
    public class PlatformsManager
    {
        private IEnumerable<Game> Games = Array.Empty<Game>();

        private readonly IPublicAPI publicApi;

        public PlatformsManager(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames(MainSettings settings)
        {
            var engines = InitializeEngines(settings);
            Games = await SynchronizeLibraries(engines);
        }

        public IEnumerable<Game> GetSynchronizedGames()
        {
            return Games;
        }

        private IEnumerable<ISyncEngine> InitializeEngines(MainSettings settings)
        {
            var engines = new List<ISyncEngine>();

            if (settings.SynchronizeXbox)
                engines.Add(new XboxSyncEngine(publicApi));

            if (settings.SynchronizeEpicGamesStore)
                engines.Add(new EpicSyncEngine(publicApi));

            if (settings.SynchronizeSteam)
                engines.Add(new SteamSyncEngine(publicApi));

            return engines;
        }

        private static async Task<IEnumerable<Game>> SynchronizeLibraries(IEnumerable<ISyncEngine> engines)
        {
            var games = new ConcurrentBag<Game>();

            await Parallel.ForEachAsync(engines, async (engine, ct) =>
            {
                await foreach (var game in engine.GetGames())
                {
                    games.Add(game);
                };
            });

            return games;
        }
    }
}
