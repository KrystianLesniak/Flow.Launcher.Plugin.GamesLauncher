namespace GamesLauncher.Platforms.SyncEngines;

internal interface ISyncEngine
{
    string PlatformName { get; }
    IAsyncEnumerable<Game> GetGames();
}
