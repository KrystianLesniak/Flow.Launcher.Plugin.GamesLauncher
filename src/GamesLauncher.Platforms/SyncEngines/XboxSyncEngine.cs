using Flow.Launcher.Plugin;
using GameFinder.Common;
using GameFinder.StoreHandlers.Xbox;
using Microsoft.WindowsAPICodePack.Shell;
using NexusMods.Paths;

namespace GamesLauncher.Platforms.SyncEngines
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
            var games = handler.FindAllGames().Where(x => x.IsGame()).Select(x => x.AsGame());

            if (!games.Any())
                yield break;

            IKnownFolder appsFolder = KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);

            foreach (var game in games)
            {
                var gameTitle = game.DisplayName;

                var shellGame = appsFolder.FirstOrDefault(x => x.ParsingName.StartsWith(game.Id.Value));

                if (shellGame != null)
                {
                    var cmd = $"shell:appsFolder\\{shellGame.ParsingName}";

                    yield return new Game(
                        Title: gameTitle,
                        Platform: PlatformName,
                        RunTask: GetGameRunTask(cmd),
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

        public Func<ActionContext, ValueTask<bool>> GetGameRunTask(string cmd)
        {
            return (context) =>
            {
                publicApi.ShellRun(cmd, "explorer.exe");

                return ValueTask.FromResult(true);
            };
        }
    }
}
