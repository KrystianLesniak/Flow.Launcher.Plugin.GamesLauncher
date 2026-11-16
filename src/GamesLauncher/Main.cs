using Flow.Launcher.Plugin;
using GamesLauncher.Common.Settings;
using GamesLauncher.Platforms;
using GamesLauncher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            var isGlobalKeyword = string.IsNullOrEmpty(query.ActionKeyword);

            return Task.FromResult(
            games
                .Where(x => _hiddenGames.IsHidden(x.InternalGameId) == false)
                .Select(EnrichGameWithActions)
                .Select(x => EnrichGameWithResultScore(x, search, isGlobalKeyword))
                .Where(x => x is not null).Select(x => x!)
                .ToList());
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            var results = new List<Result>();

            if (selectedResult.ContextData is not Game game)
                return results;

            results.Add(new Result
            {
                Title = "Hide",
                SubTitle = "The game can be unhidden in settings panel",
                IcoPath = @"Images\excludeindexpath.png",
                Glyph = new GlyphInfo(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\ued1a"),
                AsyncAction = (context) =>
                {
                    _hiddenGames.Hide(game.Title, game.SubTitle, game.InternalGameId);
                    _publicApi.ReQuery(false);
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
                        var uninstall = game.UninstallAction;

                        if (uninstall.ConfirmationMessage is not null &&
                            _publicApi.ShowMsgBox(uninstall.ConfirmationMessage, game.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                            return ValueTask.FromResult(false);

                        _ = RunUninstall();
                        return ValueTask.FromResult(true);

                        async Task RunUninstall()
                        {
                            try
                            {
                                await uninstall.UninstallTask.Invoke();
                            }
                            catch (Exception ex)
                            {
                                _publicApi.LogException(nameof(Main), $"{uninstall.Title} failed for {game.Title}", ex);
                                _publicApi.ShowMsgError($"GamesLauncher: {uninstall.Title} failed", game.Title);
                            }
                        }
                    }
                });
            }

            return results;
        }

        public Control CreateSettingPanel()
        {
            return new SettingsView(_settings, _hiddenGames, _publicApi);
        }

        private Game EnrichGameWithActions(Game game)
        {
            game.AsyncAction ??= async (context) =>
            {
                _lastPlayedGames.AddLaunchedGameToLastPlayed(game.InternalGameId);
                await game.RunTask.Invoke();
                return true;
            };

            return game;
        }

        private Result? EnrichGameWithResultScore(Game game, string search, bool isGlobalKeyword)
        {
            var result = game.Clone();

            if (!isGlobalKeyword && string.IsNullOrWhiteSpace(search))   //When there is no global keyword set and no search query typed prioritze last played games.
            {
                result.Score = _lastPlayedGames.GetResultScoreByOrder(game.InternalGameId);
                result.TitleHighlightData = Array.Empty<int>();
            }
            else
            {
                var fuzzySearch = _publicApi.FuzzySearch(search, game.Title);
                result.Score = fuzzySearch.Score;
                result.TitleHighlightData = fuzzySearch.MatchData;
            }

            if (isGlobalKeyword)
                return result.Score > 0 ? result : null;
            else
                return result;
        }
    }
}