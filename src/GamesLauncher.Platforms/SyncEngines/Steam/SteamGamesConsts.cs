using System.Collections.Immutable;

namespace GamesLauncher.Platforms.SyncEngines.Steam
{
    internal static class SteamGamesConsts
    {
        //TODO: When hiding games from list feature will be implemented this will be no longer needed
        internal static readonly IImmutableList<uint> AppIdsToIgnore = new List<uint>()
        {
            228980  //Steamworks Common Redistributables
        }
        .ToImmutableList();
    }
}
