﻿using Flow.Launcher.Plugin;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models;
using GamesLauncher.Platforms.SyncEngines.Common.Interfaces;

namespace GamesLauncher.Platforms.SyncEngines.Ubisoft
{
    internal class UbisoftSyncEngine : ISyncEngine
    {
        public string PlatformName => "Ubisoft Connect";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();


        private readonly string UbisoftConnectUninstallRegistryKeyStart = "Uplay Install ";

        private readonly IPublicAPI publicApi;

        public UbisoftSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {
            var ubiUninstallPrograms = (await ControlPanelUninstall.GetPrograms())
                                .Where(x => x.SubKeyName.StartsWith(UbisoftConnectUninstallRegistryKeyStart));

            if (!ubiUninstallPrograms.Any())
                return;

            var syncedGames = new List<Game>();

            foreach (var ubiUninstallProgram in ubiUninstallPrograms)
            {
                syncedGames.Add(new Game(
                    title: ubiUninstallProgram.DisplayName,
                    platform: PlatformName,
                    runTask: GetGameRunTask(ubiUninstallProgram),
                    iconPath: ubiUninstallProgram.DisplayIcon
                //TODO: Add uninstall action
                ));
            }

            SynchronizedGames = syncedGames;

            await Task.CompletedTask;
        }

        private Func<Task> GetGameRunTask(UninstallProgram ubiUninstallProgram)
        {
            var gameId = ubiUninstallProgram.SubKeyName.Replace(UbisoftConnectUninstallRegistryKeyStart, string.Empty).Trim();
            var uriString = $"uplay://launch/{gameId}/0";

            return async () =>
            {
                publicApi.OpenAppUri(new Uri(uriString));
                await Task.CompletedTask;
            };
        }
    }
}
