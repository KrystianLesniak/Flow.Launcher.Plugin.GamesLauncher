using Flow.Launcher.Plugin;

namespace GamesLauncher.Platforms.SyncEngines.Common
{
    internal static class EngineHelpers
    {
        internal static async Task WaitUntilGone(Func<Task<bool>> isGone)
        {
            for (int i = 0; i < 12 && !await isGone(); i++)
                await Task.Delay(TimeSpan.FromSeconds(5));
        }

        internal static void RunFile(this IPublicAPI publicApi, string path)
        {
            publicApi.ShellRun($"cd /d \"{Path.GetDirectoryName(path)}\" && start \"\" \"{Path.GetFileName(path)}\"");
        }
    }
}
