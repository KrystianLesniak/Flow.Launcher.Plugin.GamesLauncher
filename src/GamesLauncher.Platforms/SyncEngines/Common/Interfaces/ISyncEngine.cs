namespace GamesLauncher.Platforms.SyncEngines.Common.Interfaces;

internal interface ISyncEngine
{
    string PlatformName { get; }
    IEnumerable<Game> SynchronizedGames { get; }
    Task SynchronizeGames();
}
