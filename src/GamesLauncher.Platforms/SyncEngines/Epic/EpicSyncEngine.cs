using Flow.Launcher.Plugin;
using GamesLauncher.Platforms.SyncEngines.Epic.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace GamesLauncher.Platforms.SyncEngines.Epic
{
    internal class EpicSyncEngine : ISyncEngine
    {
        private readonly IPublicAPI publicApi;

        public string PlatformName => "Epic Games Launcher";

        public EpicSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async IAsyncEnumerable<Game> GetGames()
        {
            var epicGames = await GetEpicGamesFromMetadata();

            foreach (var epicGame in epicGames)
            {

                yield return new Game(
                    Title: epicGame.DisplayName,
                    RunTask: PrepareRunTask(epicGame.CatalogNamespace, epicGame.CatalogItemId, epicGame.AppName),
                    IconPath: PrepareIconPath(epicGame),
                    Platform: PlatformName,
                    IconDelegate: null
                    );
            }
        }

        private static async Task<IEnumerable<EpicGame>> GetEpicGamesFromMetadata()
        {
            var metadataDir = (string?)Registry.GetValue(EpicGamesConsts.RegKeyPath, EpicGamesConsts.RegKeyValueName, string.Empty);

            if (string.IsNullOrEmpty(metadataDir))
                return Array.Empty<EpicGame>();

            var epicGames = new List<EpicGame>();

            foreach (var metadataFile in Directory.EnumerateFiles(metadataDir, "*.item", SearchOption.AllDirectories))
            {
                var fileContent = await File.ReadAllTextAsync(metadataFile);

                var jObject = JsonConvert.DeserializeObject<JObject>(fileContent, new JsonSerializerSettings
                {
                    Error = delegate (object? sender, ErrorEventArgs args)  // When malfunctioned JSON return null
                    {
                        args.ErrorContext.Handled = true;
                    }
                });

                var game = EpicGame.CreateFromJObject(jObject);

                if (game != null)
                    epicGames.Add(game);
            }

            return epicGames;
        }

        private Func<ActionContext, ValueTask<bool>> PrepareRunTask(string catalogNamespace, string catalogItemId, string appName)
        {
            var launchUriString = $"com.epicgames.launcher://apps/{catalogNamespace}%3A{catalogItemId}%3A{appName}?action=launch&silent=true";

            return (context) =>
            {
                publicApi.OpenAppUri(new Uri(launchUriString));

                return ValueTask.FromResult(true);
            };
        }

        private static string PrepareIconPath(EpicGame epicGame)
        {
            if (epicGame.InstallLocation != null && epicGame.LaunchExecutable != null)
            {
                var exePath = Path.Combine(epicGame.InstallLocation, epicGame.LaunchExecutable);

                if (File.Exists(exePath))
                    return exePath;
            }

            return Path.Combine("Icons", "epic.png");
        }

    }
}
