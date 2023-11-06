using Flow.Launcher.Plugin;
using static Flow.Launcher.Plugin.Result;

namespace GamesLauncher.Platforms
{
    public class Game
    {
        internal Game(string title,
                       Func<ActionContext, ValueTask<bool>> runTask,
                       string? iconPath,
                       IconDelegate? iconDelegate,
                       string platform,
                       UninstallAction? uninstallAction = null)
        {
            Title = title;
            RunTask = runTask;
            IconPath = iconPath;
            IconDelegate = iconDelegate;
            Platform = platform;
            UninstallAction = uninstallAction;
        }

        public string Title { get; }
        public Func<ActionContext, ValueTask<bool>> RunTask { get; }
        public UninstallAction? UninstallAction { get; }
        public string? IconPath { get; }
        public IconDelegate? IconDelegate { get; }
        public string Platform { get; }
        public string InternalGameId => $"{Platform}_{Title}";

    }

    public class UninstallAction
    {
        public string Title { get; } = "Uninstall";

        public Func<Task> UninstallTask { get; }

        internal UninstallAction(Func<Task> uninstallTask)
        {
            UninstallTask = uninstallTask;
        }

        internal UninstallAction(Func<Task> uninstallTask, string title)
        {
            Title = title;
            UninstallTask = uninstallTask;
        }

    }
}