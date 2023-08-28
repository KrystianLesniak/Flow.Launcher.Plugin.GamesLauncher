using Flow.Launcher.Plugin;
using GamesLauncher.Common.Settings;
using GamesLauncher.Platforms;
using GamesLauncher.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace GamesLauncher
{
    public class Main : IAsyncPlugin, ISettingProvider, IAsyncReloadable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IPublicAPI _publicApi;
        private MainSettings _settings;
        private LastPlayedGames _lastPlayedGames;

        private PlatformsManager _platformsManager;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.



        public async Task InitAsync(PluginInitContext context)
        {
            _publicApi = context.API;
            _settings = _publicApi.LoadSettingJsonStorage<MainSettings>();
            _lastPlayedGames = _publicApi.LoadSettingJsonStorage<LastPlayedGames>();

            _platformsManager = new PlatformsManager(context.API);

            await ReloadDataAsync();
        }

        public async Task ReloadDataAsync()
        {
            await _platformsManager.SynchronizeGames(_settings);
        }

        public Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            var gamesQuery = _platformsManager.GetSynchronizedGames();

            var search = query.Search.Trim();

            return Task.FromResult(gamesQuery.Select(x => CreateResultFromGame(x, search)).ToList());
        }

        public Control CreateSettingPanel()
        {
            return new SettingsView(_settings);
        }

        private Result CreateResultFromGame(Game game, string search)
        {
            var result = new Result
            {
                Title = game.Title,
                AsyncAction = game.RunTask,
                IcoPath = game.IconPath,
                Icon = game.IconDelegate,
                SubTitle = game.Platform,
            };

            result.AsyncAction = (context) =>
            {
                _lastPlayedGames.AddLaunchedGameToLastPlayed(game.InternalGameId);
                return game.RunTask.Invoke(context);
            };

            //Get score
            if (string.IsNullOrWhiteSpace(search))   //When there is no search query display 10 last played games
            {
                result.Score = _lastPlayedGames.GetResultScoreByOrder(game.InternalGameId);
            }
            else
            {
                var fuzzySearch = _publicApi.FuzzySearch(search, game.Title);
                result.Score = fuzzySearch.Score;
                result.TitleHighlightData = fuzzySearch.MatchData;
            }

            return result;
        }

    }
}