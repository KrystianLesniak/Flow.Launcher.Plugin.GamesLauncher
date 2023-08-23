using Flow.Launcher.Plugin.GamesLauncher.Handlers;
using Flow.Launcher.Plugin.GamesLauncher.Models;
using GameFinder.StoreHandlers.Xbox;
using Microsoft.WindowsAPICodePack.Shell;
using NexusMods.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.GamesLauncher.SyncEngines
{
    internal class XboxSyncEngine : ISyncEngine
    {
        public string PlatformName => "Xbox";

        private readonly XboxHandler handler = new(FileSystem.Shared);
        private readonly Guid FODLERID_AppsFolder = new("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");

        private readonly GameHandler gameHandler;

        public XboxSyncEngine(GameHandler lastPlayedHandler)
        {
            this.gameHandler = lastPlayedHandler;
        }

        public async IAsyncEnumerable<Game> GetGames()
        {
            var games = handler.FindAllGames().Where(x => x.IsT0).Select(x => x.AsT0);

            if (!games.Any())
                yield break;

            IKnownFolder appsFolder = KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);

            foreach (var game in games)
            {
                var gameTitle = game.DisplayName;

                var shellGame = appsFolder.FirstOrDefault(x => x.Name == gameTitle);
                if (shellGame != null)
                {
                    var cmd = $"shell:appsFolder\\{shellGame.ParsingName}";

                    yield return new Game(
                        Title: gameTitle,
                        Platform: PlatformName,
                        RunTask: gameHandler.GetGameRunTaskByShell(PlatformName, gameTitle, cmd),
                        IconPath: null,
                        IconDelegate: delegate ()
                        {
                            return shellGame.Thumbnail.LargeBitmapSource;
                        }
                        );
                }
            }

            await Task.CompletedTask;
        }
    }
}
