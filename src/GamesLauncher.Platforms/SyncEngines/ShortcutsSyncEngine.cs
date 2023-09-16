using Flow.Launcher.Plugin;
using GamesLauncher.Common;

namespace GamesLauncher.Platforms.SyncEngines
{
    public class ShortcutsSyncEngine : ISyncEngine

    {
        public string PlatformName => "Shortcut";

        private readonly IPublicAPI publicApi;
        private readonly DirectoryInfo shortcutsDirectory;

        public ShortcutsSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
            shortcutsDirectory = Directory.CreateDirectory(Paths.CustomShortcutsDirectory);
        }

        public async IAsyncEnumerable<Game> GetGames()
        {
            string[] shortcutExtensions = { "*.url", "*.lnk" };

            foreach (var shortcutExtension in shortcutExtensions)
            {
                foreach (var shortcut in shortcutsDirectory.EnumerateFiles(shortcutExtension, SearchOption.AllDirectories))
                {
                    yield return new Game(
                        Title: Path.GetFileNameWithoutExtension(shortcut.FullName),
                        RunTask: GetGameRunTask(shortcut.FullName),
                        IconPath: await GetIconPath(shortcut),
                        IconDelegate: null,
                        Platform: PlatformName
                        );
                }
            }
        }

        private Func<ActionContext, ValueTask<bool>> GetGameRunTask(string fullPath)
        {
            return (context) =>
            {
                publicApi.ShellRun($"start \"\" \"{fullPath}\"");

                return ValueTask.FromResult(true);
            };
        }

        private static async Task<string?> GetIconPath(FileInfo fileInfo)
        {
            if (fileInfo.Extension == ".lnk")
            {
                return fileInfo.FullName;
            }

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

            return null;
        }

    }
}
