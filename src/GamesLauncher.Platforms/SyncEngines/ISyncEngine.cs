namespace GamesLauncher.Platforms.SyncEngines;

internal interface ISyncEngine
{
    string PlatformName { get; }
    IEnumerable<Game> SynchronizedGames { get; }
    Task SynchronizeGames();
}
