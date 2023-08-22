using Flow.Launcher.Plugin.GamesLauncher.Models;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Xbox;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.WindowsAPICodePack.Shell;
using NexusMods.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Flow.Launcher.Plugin.GamesLauncher.SyncEngines
{
    internal class XboxSyncEngine : ISyncEngine
    {
        public string PlatformName => "Xbox";

        private readonly XboxHandler handler = new(FileSystem.Shared);

        private readonly Guid FODLERID_AppsFolder = new("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
        private readonly IPublicAPI publicApi;

        public XboxSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async IAsyncEnumerable<Game> GetGames()
        {
            var games = handler.FindAllGames().Where(x=>x.IsT0).Select(x=>x.AsT0);

            if(!games.Any())
                yield break;

            IKnownFolder appsFolder = KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);

            foreach (var game in games)
            {
                var shellGame = appsFolder.FirstOrDefault(x => x.Name == game.DisplayName);
                if (shellGame != null)
                {
                    yield return new Game(
                        Title: game.DisplayName,
                        Platform: PlatformName,
                        RunTask: (context) =>
                        {
                            publicApi.ShellRun($"shell:appsFolder\\{shellGame.ParsingName}", "explorer.exe");

                            return ValueTask.FromResult(true);
                        },
                        IconPath: null,
                        IconDelegate: delegate()
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
