using System.Threading.Tasks;
using System;

namespace Flow.Launcher.Plugin.GamesLauncher.Models
{
    public record Game(string Title,
                       Func<ActionContext, ValueTask<bool>> RunTask,
                       string IconPath,
                       string Platform
                       );
}