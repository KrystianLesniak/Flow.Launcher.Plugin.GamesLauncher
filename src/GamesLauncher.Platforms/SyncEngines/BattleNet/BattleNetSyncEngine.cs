using Flow.Launcher.Plugin;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models;
using GamesLauncher.Platforms.SyncEngines.Common;
using GamesLauncher.Platforms.SyncEngines.Common.Interfaces;

namespace GamesLauncher.Platforms.SyncEngines.BattleNet
{
    internal class BattleNetSyncEngine : ISyncEngine
    {
        public string PlatformName => "Battle.net";
        public IEnumerable<Game> SynchronizedGames { get; private set; } = Array.Empty<Game>();

        private static readonly string ProductDbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net", "Agent", "product.db");

        // Battle.net client and its helper components, not games
        private static readonly string[] NonGameProductCodes = ["agent", "bna", "bdko", "bts"];

        // product.db has no display names and not every game gets an uninstall registry entry (e.g. wowf)
        private static readonly Dictionary<string, string> KnownGameNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["wow"] = "World of Warcraft",
            ["wow_classic"] = "World of Warcraft Classic",
            ["wow_classic_era"] = "World of Warcraft Classic Era",
            ["wowf"] = "World of Warcraft: Forever",
            ["w3"] = "Warcraft III: Reforged",
            ["pro"] = "Overwatch 2",
            ["d3"] = "Diablo III",
            ["fenris"] = "Diablo IV",
            ["osi"] = "Diablo II: Resurrected",
            ["s1"] = "StarCraft: Remastered",
            ["s2"] = "StarCraft II",
            ["hs_beta"] = "Hearthstone",
            ["hero"] = "Heroes of the Storm",
        };

        private readonly IPublicAPI publicApi;

        public BattleNetSyncEngine(IPublicAPI publicApi)
        {
            this.publicApi = publicApi;
        }

        public async Task SynchronizeGames()
        {
            if (!File.Exists(ProductDbPath))
                return;

            var products = ProductDbReader.Read(await File.ReadAllBytesAsync(ProductDbPath));
            var uninstallPrograms = (await ControlPanelUninstall.GetPrograms()).ToList();

            var clientPath = products.FirstOrDefault(x => x.ProductCode == "bna")?.InstallPath;
            var clientExe = clientPath is null ? null : Path.Combine(clientPath, "Battle.net.exe");

            SynchronizedGames = products
                .Where(x => x.Installed && !NonGameProductCodes.Contains(x.ProductCode) && Directory.Exists(x.InstallPath))
                .Select(x => MapToGame(x, FindUninstallProgram(uninstallPrograms, x.Uid), clientExe))
                .ToList();
        }

        private Game MapToGame(BattleNetProduct product, UninstallProgram? uninstallProgram, string? clientExe)
        {
            var title = uninstallProgram?.DisplayName is { Length: > 0 } displayName
                ? displayName
                : KnownGameNames.GetValueOrDefault(product.ProductCode) ?? Path.GetFileName(product.InstallPath.TrimEnd('/', '\\'));

            return new Game(
                title: title,
                platform: PlatformName,
                runTask: GetGameRunTask(product),
                iconPath: uninstallProgram?.DisplayIconPath ?? clientExe,
                uninstallAction: GetUninstallAction(uninstallProgram, product.Uid));
        }

        private static UninstallProgram? FindUninstallProgram(IEnumerable<UninstallProgram> programs, string uid)
        {
            // Trailing space so "--uid=w3" doesn't match "--uid=w3_legacy"
            return programs.FirstOrDefault(x => x.UninstallCommand is not null && (x.UninstallCommand + " ").Contains($"--uid={uid} "));
        }

        private Func<Task> GetGameRunTask(BattleNetProduct product)
        {
            var uri = new Uri($"battlenet://game/{product.ProductCode}");

            return async () =>
            {
                publicApi.OpenAppUri(uri);
                await Task.CompletedTask;
            };
        }

        private UninstallAction? GetUninstallAction(UninstallProgram? uninstallProgram, string uid)
        {
            var uninstallCommand = uninstallProgram?.UninstallCommand;

            if (uninstallCommand == null) return null;

            return new UninstallAction(async () =>
            {
                publicApi.ShellRun(uninstallCommand);

                await EngineHelpers.WaitUntilGone(async () =>
                    FindUninstallProgram(await ControlPanelUninstall.GetPrograms(), uid) is null);

                await SynchronizeGames();
            });
        }
    }
}
