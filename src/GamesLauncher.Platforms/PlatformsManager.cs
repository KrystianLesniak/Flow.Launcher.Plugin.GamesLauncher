using Flow.Launcher.Plugin;
using GamesLauncher.Common.Settings;
using GamesLauncher.Platforms.SyncEngines;
using GamesLauncher.Platforms.SyncEngines.Amazon;
using GamesLauncher.Platforms.SyncEngines.Epic;
using GamesLauncher.Platforms.SyncEngines.Steam;
using System.Diagnostics;

namespace GamesLauncher.Platforms
{
    public class PlatformsManager
    {
        public IEnumerable<Game> AllSynchronizedGames => Engines.SelectMany(x => x.SynchronizedGames);

        private IEnumerable<ISyncEngine> Engines { get; set; } = Array.Empty<ISyncEngine>();

        private readonly IPublicAPI publicApi;

        public PlatformsManager(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames(MainSettings settings)
        {
#if DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif

            Engines = InitializeEngines(settings);
            await Parallel.ForEachAsync(Engines, async (engine, ct) =>
            {
                await engine.SynchronizeGames();
            });

#if DEBUG
            stopwatch.Stop();
            publicApi.LogInfo(nameof(PlatformsManager), $"GamesLauncher: Games synchronization time: {stopwatch.Elapsed}");
#endif
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

            if (settings.SynchronizeAmazon)
                engines.Add(new AmazonSyncEngine(publicApi));

            engines.Add(new ShortcutsSyncEngine(publicApi));

            return engines;
        }
    }
}
