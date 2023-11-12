using Flow.Launcher.Plugin;

namespace GamesLauncher.Platforms
{
    public class Game : Result
    {
        public new string SubTitle => Platform;

        internal Game(string title,
                       string platform,
                       Func<Task> runTask,
                       string? iconPath,
                       IconDelegate? iconDelegate = null,
                       UninstallAction? uninstallAction = null)
        {
            Title = title;
            RunTask = runTask;
            IcoPath = iconPath;
            Icon = iconDelegate;
            Platform = platform;
            UninstallAction = uninstallAction;
        }
        public string InternalGameId => $"{Platform}_{Title}";
        public Func<Task> RunTask { get; set; }
        public UninstallAction? UninstallAction { get; }
        public string Platform { get; }

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