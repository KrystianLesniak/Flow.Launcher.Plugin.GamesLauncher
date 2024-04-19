namespace GamesLauncher.Common.Settings
{
    public class LastPlayedGames
    {
        public List<string> OrderedGames { get; set; } = new List<string>();

        public void AddLaunchedGameToLastPlayed(string internalGameId)
        {
            if (OrderedGames.Contains(internalGameId))
            {
                OrderedGames.Remove(internalGameId);
                OrderedGames.Add(internalGameId);
            }
            else
            {
                OrderedGames.Add(internalGameId);

                if (OrderedGames.Count > 100)
                    OrderedGames.RemoveAt(0);
            }
        }

        public int GetResultScoreByOrder(string internalGameId)
        {
            var index = OrderedGames.LastIndexOf(internalGameId);

            return (++index) * 1000;
        }

    }
}
