using Flow.Launcher.Plugin.GamesLauncher.Models;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.GamesLauncher.SyncEngines
{
    internal interface ISyncEngine
    {
        string PlatformName { get; }
        IAsyncEnumerable<Game> GetGames();
    }
}
