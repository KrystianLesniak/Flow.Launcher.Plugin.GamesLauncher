namespace GamesLauncher.Common.Settings
{
    public class HidenGames
    {
        public List<HidenGame> Items { get; set; } = new List<HidenGame>(); //This property and class needs to be public to be retrieved by FL Settings

        public void Hide(string title, string platform, string internalGameId)
        {
            if (!Items.Any(x => x.InternalGameId == internalGameId))
            {
                Items.Add(new HidenGame
                {
                    InternalGameId = internalGameId,
                    Platform = platform,
                    Title = title
                });
            }
        }

        public void Unhide(string internalGameId)
        {
            if (Items.Any(x => x.InternalGameId == internalGameId))
            {
                Items.Remove(Items.First(x => x.InternalGameId == internalGameId));
            }
        }

        public bool IsHiden(string internalGameId)
        {
            return Items.Any(x => x.InternalGameId == internalGameId);
        }
    }

    public class HidenGame
    {
        public string Title { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string InternalGameId { get; set; } = string.Empty;
    }

}
