using Flow.Launcher.Plugin;
using GamesLauncher.Common;

namespace GamesLauncher.Platforms.SyncEngines
{
    public class ShortcutsSyncEngine : ISyncEngine
    {
        public string PlatformName => "Shortcut";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();

        private readonly IPublicAPI publicApi;
        private readonly DirectoryInfo shortcutsDirectory;

        public ShortcutsSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
            shortcutsDirectory = Directory.CreateDirectory(Paths.CustomShortcutsDirectory);
        }

        public async Task SynchronizeGames()
        {
            string[] shortcutExtensions = { "*.url", "*.lnk" };
            var syncedGames = new List<Game>();

            foreach (var shortcutExtension in shortcutExtensions)
            {
                foreach (var shortcut in shortcutsDirectory.EnumerateFiles(shortcutExtension, SearchOption.AllDirectories))
                {
                    syncedGames.Add(new Game(
                        title: Path.GetFileNameWithoutExtension(shortcut.FullName),
                        runTask: GetGameRunTask(shortcut.FullName),
                        iconPath: await GetIconPath(shortcut),
                        iconDelegate: null,
                        platform: PlatformName,
                        uninstallAction: new(
                            GetShortcutDeleteTask(shortcut.FullName),
                            "Delete Shortcut"
                            )
                        ));
                }
            }

            SynchronizedGames = syncedGames;
        }

        private Func<Task> GetGameRunTask(string fullPath)
        {
            return async () =>
            {
                publicApi.ShellRun($"start \"\" \"{fullPath}\"");
                await Task.CompletedTask;
            };
        }

        private Func<Task> GetShortcutDeleteTask(string fullPath)
        {
            return async () =>
            {
                File.Delete(fullPath);
                await SynchronizeGames();
            };
        }

        private static async Task<string?> GetIconPath(FileInfo fileInfo)
        {
            if (fileInfo.Extension == ".url")
            {
                await foreach (var line in File.ReadLinesAsync(fileInfo.FullName))
                {
                    if (line.Trim().StartsWith("IconFile="))
                    {
                        return line.Replace("IconFile=", "").Trim();
                    }
                }
            }

            return fileInfo.FullName;
        }

    }
}
