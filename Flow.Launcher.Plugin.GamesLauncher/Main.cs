using Flow.Launcher.Plugin.GamesLauncher.Models;
using Flow.Launcher.Plugin.GamesLauncher.SyncEngines;
using Flow.Launcher.Plugin.GamesLauncher.SyncEngines.EpicSyncEngine;
using Flow.Launcher.Plugin.GamesLauncher.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace Flow.Launcher.Plugin.GamesLauncher
{
    public class GamesLauncher : IAsyncPlugin, ISettingProvider, IAsyncReloadable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private PluginInitContext _context;
        private Settings _settings;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        private IEnumerable<Game> _games = Array.Empty<Game>();

        public async Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();

            await SynchronizeLibrary();
        }

        public Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            var gamesQuery = _games;

            var search = query.Search.Trim();

            return Task.FromResult(gamesQuery.Select(x => MapGameToResult(x, search)).ToList());
        }

        public async Task ReloadDataAsync()
        {
            await SynchronizeLibrary();
        }

        public Control CreateSettingPanel()
        {
            return new SettingsView(_settings);
        }
        private Result MapGameToResult(Game game, string search)
        {
            return new Result
            {
                Title = game.Title,
                AsyncAction = game.RunTask,
                IcoPath = game.IconPath,
                Icon = game.IconDelegate,
                SubTitle = game.Platform,
                Score = string.IsNullOrWhiteSpace(search) ? 0 : _context.API.FuzzySearch(search, game.Title).Score
            };
        }

        private async Task SynchronizeLibrary()
        {
            var engines = InitializeEngines();

            _games = await GetGamesFromEngines(engines);
        }

        private IEnumerable<ISyncEngine> InitializeEngines()
        {
            var engines = new List<ISyncEngine>();

            if(_settings.SynchronizeXbox)
                engines.Add(new XboxSyncEngine(_context.API));

            if (_settings.SynchronizeEpicGamesStore)
                engines.Add(new EpicSyncEngine(_context.API));

            if (_settings.SynchronizeSteam)
                engines.Add(new SteamSyncEngine(_context.API));

            return engines;
        }

        private static async Task<IEnumerable<Game>> GetGamesFromEngines(IEnumerable<ISyncEngine> engines)
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