﻿using Flow.Launcher.Plugin.GamesLauncher.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.GamesLauncher.SyncEngines.EpicSyncEngine.Models;

namespace Flow.Launcher.Plugin.GamesLauncher.SyncEngines.EpicSyncEngine
{
    internal class EpicSyncEngine : ISyncEngine
    {
        public string PlatformName => "Epic Games Launcher";


        private readonly IPublicAPI publicApi;

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
                    RunTask: (context) =>
                    {
                        var uriString = PrepareLaunchUri(epicGame.CatalogNamespace, epicGame.CatalogItemId, epicGame.AppName);

                        publicApi.OpenAppUri(new Uri(uriString));

                        return ValueTask.FromResult(true);
                    },
                    IconPath: PrepareIconPath(epicGame),
                    Platform: PlatformName
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
                using FileStream jsonFileStream = File.OpenRead(metadataFile);
                var jsonNode = await JsonSerializer.DeserializeAsync<JsonNode>(jsonFileStream);

                var game = EpicGame.CreateFromJsonNode(jsonNode);

                if (game != null)
                    epicGames.Add(game);

            }

            return epicGames;
        }

        private string PrepareIconPath(EpicGame epicGame)
        {
            if(epicGame.InstallLocation != null && epicGame.LaunchExecutable != null)
            {
                var exePath = Path.Combine(epicGame.InstallLocation, epicGame.LaunchExecutable);

                if (File.Exists(exePath))
                    return exePath;
            }

            return Path.Combine("Icons", "epic.png");
        }

        private static string PrepareLaunchUri(string catalogNamespace, string catalogItemId, string appName)
        {
            return $"com.epicgames.launcher://apps/{catalogNamespace}%3A{catalogItemId}%3A{appName}?action=launch&silent=true";
        }

    }
}
