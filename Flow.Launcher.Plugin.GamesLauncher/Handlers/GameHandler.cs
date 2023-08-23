using Flow.Launcher.Plugin.GamesLauncher.Data;
using System;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.GamesLauncher.Handlers
{
    internal class GameHandler
    {
        private readonly IPublicAPI publicApi;

        private readonly LastPlayedGames lastPlayedGames;

        public GameHandler(LastPlayedGames lastPlayedGames, IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
            this.lastPlayedGames = lastPlayedGames;
        }

        public Func<ActionContext, ValueTask<bool>> GetGameRunTaskByUri(string platform, string gameTitle, string uriString)
        {
            return (context) =>
            {
                publicApi.OpenAppUri(new Uri(uriString));
                AddLaunchedGameToLastPlayed(platform, gameTitle);

                return ValueTask.FromResult(true);
            };
        }

        public Func<ActionContext, ValueTask<bool>> GetGameRunTaskByShell(string platform, string gameTitle, string cmd, string shell = "explorer.exe")
        {
            return (context) =>
            {
                publicApi.ShellRun(cmd, shell);
                AddLaunchedGameToLastPlayed(platform, gameTitle);

                return ValueTask.FromResult(true);
            };
        }

        private void AddLaunchedGameToLastPlayed(string platform, string gameTitle)
        {
            var gameId = PrepareGameIdentifier(platform, gameTitle);

            if (lastPlayedGames.Contains(gameId))
            {
                lastPlayedGames.Remove(gameId);
                lastPlayedGames.Add(gameId);
            }
            else
            {
                lastPlayedGames.Add(gameId);

                if (lastPlayedGames.Count > 10)
                    lastPlayedGames.RemoveAt(0);
            }
        }

        public int GetGameResultScoreByOrder(string platform, string gameTitle)
        {
            var gameId = PrepareGameIdentifier(platform, gameTitle);

            var index = lastPlayedGames.LastIndexOf(gameId);

            return (++index) * 100;
        }

        private static string PrepareGameIdentifier(string platform, string gameTitle)
            => $"{platform}_{gameTitle}";
    }
}
