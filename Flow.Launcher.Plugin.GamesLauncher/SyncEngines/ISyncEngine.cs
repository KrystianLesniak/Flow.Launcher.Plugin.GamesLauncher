using System.Collections.Generic;
using Flow.Launcher.Plugin.GamesLauncher.Models;

namespace Flow.Launcher.Plugin.GamesLauncher.SyncEngines
{
    internal interface ISyncEngine
    {
        string PlatformName { get; }
        IAsyncEnumerable<Game> GetGames();
    }
}
