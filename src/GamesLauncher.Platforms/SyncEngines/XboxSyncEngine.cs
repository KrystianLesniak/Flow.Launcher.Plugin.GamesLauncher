using Flow.Launcher.Plugin;
using GameFinder.Common;
using GameFinder.StoreHandlers.Xbox;
using Microsoft.WindowsAPICodePack.Shell;
using NexusMods.Paths;
using static Flow.Launcher.Plugin.Result;

namespace GamesLauncher.Platforms.SyncEngines
{
    internal class XboxSyncEngine : ISyncEngine
    {
        public string PlatformName => "Xbox";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();

        private readonly XboxHandler handler = new(FileSystem.Shared);
        private readonly Guid FODLERID_AppsFolder = new("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");

        private readonly IPublicAPI publicApi;

        public XboxSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {
            var games = handler.FindAllGames().Where(x => x.IsGame()).Select(x => x.AsGame());

            if (!games.Any())
                return;

            var syncedGames = new List<Game>();

            IKnownFolder appsFolder = KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);

            foreach (var game in games)
            {
                var shellGame = appsFolder.FirstOrDefault(x => x.ParsingName.StartsWith(game.Id.Value));

                if (shellGame != null)
                {
                    syncedGames.Add(MapShellObjectToGame(shellGame));
                }
            }

            SynchronizedGames = syncedGames;

            await Task.CompletedTask;
        }

        private Game MapShellObjectToGame(ShellObject shellGame)
        {
            var cmd = $"shell:appsFolder\\{shellGame.ParsingName}";

            var iconDelegate = GetIconDelegate(shellGame);

            return new Game(
                title: shellGame.Name,
                platform: PlatformName,
                runTask: GetGameRunTask(cmd),
                iconPath: iconDelegate is null ? Path.Combine("Icons", "xbox.png") : null,
                iconDelegate: iconDelegate
                );
        }

        private Func<ActionContext, ValueTask<bool>> GetGameRunTask(string cmd)
        {
            return (context) =>
            {
                publicApi.ShellRun(cmd, "explorer.exe");

                return ValueTask.FromResult(true);
            };
        }

        private static IconDelegate? GetIconDelegate(ShellObject shellGame)
        {
            var bitmapSource = shellGame.Thumbnail.LargeBitmapSource;

            if (bitmapSource == null || bitmapSource.CanFreeze == false)
                return null;

            bitmapSource.Freeze(); //This is needed. Otherwise FL throws exception for some users.

            return delegate ()
            {
                return bitmapSource;
            };
        }
    }
}
