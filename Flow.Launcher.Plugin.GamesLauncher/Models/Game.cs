using System;
using System.Threading.Tasks;
using static Flow.Launcher.Plugin.Result;

namespace Flow.Launcher.Plugin.GamesLauncher.Models
{
    public record Game(string Title,
                       Func<ActionContext, ValueTask<bool>> RunTask,
                       string? IconPath,
                       IconDelegate? IconDelegate,
                       string Platform
                       );
}