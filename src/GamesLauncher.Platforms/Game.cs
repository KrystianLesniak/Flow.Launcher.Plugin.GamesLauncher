using Flow.Launcher.Plugin;
using GamesLauncher.Common.Extensions;

namespace GamesLauncher.Platforms
{
    public class Game : Result
    {
        internal Game(string title,
                       string platform,
                       Func<Task> runTask,
                       string? iconPath,
                       IconDelegate? iconDelegate = null,
                       UninstallAction? uninstallAction = null)
        {
            Title = title;
            SubTitle = platform;
            RunTask = runTask;
            IcoPath = iconPath;
            Icon = iconDelegate;
            UninstallAction = uninstallAction;
            ContextData = this;
        }
        public string InternalGameId => this.GetInternalGameId();
        public Func<Task> RunTask { get; set; }
        public UninstallAction? UninstallAction { get; }

    }

    public class UninstallAction
    {
        public string Title { get; } = "Uninstall";

        public Func<Task> UninstallTask { get; }

        internal UninstallAction(Func<Task> uninstallTask)
        {
            UninstallTask = uninstallTask;
        }

        public string? ConfirmationMessage { get; }

        internal UninstallAction(Func<Task> uninstallTask, string title, string? confirmationMessage = null)
        {
            Title = title;
            UninstallTask = uninstallTask;
            ConfirmationMessage = confirmationMessage;
        }

    }
}