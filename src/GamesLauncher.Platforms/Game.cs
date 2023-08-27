using Flow.Launcher.Plugin;
using static Flow.Launcher.Plugin.Result;

namespace GamesLauncher.Platforms
{
    public class Game
    {
        internal Game(string Title,
                       Func<ActionContext, ValueTask<bool>> RunTask,
                       string? IconPath,
                       IconDelegate? IconDelegate,
                       string Platform)
        {
            this.Title = Title;
            this.RunTask = RunTask;
            this.IconPath = IconPath;
            this.IconDelegate = IconDelegate;
            this.Platform = Platform;
        }

        public string Title { get; }
        public Func<ActionContext, ValueTask<bool>> RunTask { get; }
        public string? IconPath { get; }
        public IconDelegate? IconDelegate { get; }
        public string Platform { get; }
        public string InternalGameId => $"{Platform}_{Title}";

    }
}