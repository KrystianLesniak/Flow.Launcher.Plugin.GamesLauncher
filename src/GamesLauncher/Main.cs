using Flow.Launcher.Plugin;
using GamesLauncher.Common.Settings;
using GamesLauncher.Platforms;
using GamesLauncher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace GamesLauncher
{
    public class Main : IAsyncPlugin, ISettingProvider, IAsyncReloadable, IContextMenu
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private MainSettings _settings;
        private LastPlayedGames _lastPlayedGames;
        private HiddenGames _hiddenGames;

        private IPublicAPI _publicApi;
        private PlatformsManager _platformsManager;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.



        public async Task InitAsync(PluginInitContext context)
        {
            _publicApi = context.API;
            _settings = _publicApi.LoadSettingJsonStorage<MainSettings>();
            _lastPlayedGames = _publicApi.LoadSettingJsonStorage<LastPlayedGames>();
            _hiddenGames = _publicApi.LoadSettingJsonStorage<HiddenGames>();

            _platformsManager = new PlatformsManager(context.API);

            await ReloadDataAsync();
        }

        public async Task ReloadDataAsync()
        {
            await _platformsManager.SynchronizeGames(_settings);
        }

        public Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            var games = _platformsManager.AllSynchronizedGames;

            var search = query.Search.Trim();

            return Task.FromResult(
            games
                .Where(x => _hiddenGames.IsHidden(x.InternalGameId) == false)
                .Select(x => EnrichGameWithQueryAndAction(x, search))
                .ToList());
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            var results = new List<Result>();

            if (selectedResult is not Game game)
                return results;

            results.Add(new Result
            {
                Title = "Hide",
                SubTitle = "The game can be unhidden in settings panel",
                IcoPath = @"Images\excludeindexpath.png",
                Glyph = new GlyphInfo(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\ued1a"),
                AsyncAction = (context) =>
                {
                    _hiddenGames.Hide(game.Title, game.Platform, game.InternalGameId);
                    return ValueTask.FromResult(false);
                }
            });

            if (game.UninstallAction is not null)
            {
                results.Add(new Result
                {
                    Title = game.UninstallAction.Title,
                    IcoPath = @"Images\deletefilefolder.png",
                    Glyph = new GlyphInfo(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\ue74d"),
                    AsyncAction = (context) =>
                    {
                        _ = game.UninstallAction.UninstallTask.Invoke();
                        return ValueTask.FromResult(true);
                    }
                });
            }

            return results;
        }

        public Control CreateSettingPanel()
        {
            return new SettingsView(_settings, _hiddenGames, _publicApi);
        }

        private Result EnrichGameWithQueryAndAction(Game game, string search)
        {
            game.AsyncAction ??= async (context) =>
            {
                _lastPlayedGames.AddLaunchedGameToLastPlayed(game.InternalGameId);
                await game.RunTask.Invoke();
                return true;
            };

            //Get score
            if (string.IsNullOrWhiteSpace(search))   //When there is no search query display 10 last played games
            {
                game.Score = _lastPlayedGames.GetResultScoreByOrder(game.InternalGameId);
                game.TitleHighlightData = Array.Empty<int>();
            }
            else
            {
                var fuzzySearch = _publicApi.FuzzySearch(search, game.Title);
                game.Score = fuzzySearch.Score;
                game.TitleHighlightData = fuzzySearch.MatchData;
            }

            return game;
        }
    }
}