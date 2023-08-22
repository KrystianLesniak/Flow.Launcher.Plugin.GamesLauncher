using System;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.GamesLauncher.Models
{
    public record Game(string Title,
                       Func<ActionContext, ValueTask<bool>> RunTask,
                       string IconPath,
                       string Platform
                       );
}