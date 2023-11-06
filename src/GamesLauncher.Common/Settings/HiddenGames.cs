namespace GamesLauncher.Common.Settings
{
    public class HiddenGames
    {
        public List<HiddenGame> Items { get; set; } = new List<HiddenGame>() //This property and class needs to be public to be retrieved by FL Settings
        {
            new HiddenGame
            {
                InternalGameId = "Steam_Steamworks Common Redistributables",
                Platform = "Steam",
                Title = "Steamworks Common Redistributables"
            }
        };

        public void Hide(string title, string platform, string internalGameId)
        {
            if (!Items.Any(x => x.InternalGameId == internalGameId))
            {
                Items.Add(new HiddenGame
                {
                    InternalGameId = internalGameId,
                    Platform = platform,
                    Title = title
                });
            }
        }

        public void Unhide(HiddenGame hiddenGame)
        {
            Items.Remove(hiddenGame);
        }

        public bool IsHidden(string internalGameId)
        {
            return Items.Any(x => x.InternalGameId == internalGameId);
        }
    }

    public class HiddenGame
    {
        public string Title { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string InternalGameId { get; set; } = string.Empty;
    }

}
