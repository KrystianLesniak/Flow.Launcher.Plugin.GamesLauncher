using Flow.Launcher.Plugin;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall;
using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models;
using GamesLauncher.Platforms.SyncEngines.Common;
using GamesLauncher.Platforms.SyncEngines.Common.Interfaces;
using System.Diagnostics;

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

        // product.db has no display names (and not every game has an uninstall registry entry, e.g. wowf), and
        // Battle.net.exe --exec="launch <id>" needs a case-sensitive game family id that differs from the product code
        private static readonly Dictionary<string, (string Name, string LaunchId)> KnownGames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["wow"] = ("World of Warcraft", "WoW"),
            ["wow_classic"] = ("World of Warcraft Classic", "WoWC"),
            ["wowf"] = ("World of Warcraft: Forever", "WoWF"),
            ["w3"] = ("Warcraft III: Reforged", "W3"),
            ["pro"] = ("Overwatch 2", "Pro"),
            ["d3"] = ("Diablo III", "D3"),
            ["d3cn"] = ("Diablo III", "D3CN"),
            ["fenris"] = ("Diablo IV", "Fen"),
            ["osi"] = ("Diablo II: Resurrected", "OSI"),
            ["anbs"] = ("Diablo Immortal", "ANBS"),
            ["s1"] = ("StarCraft: Remastered", "S1"),
            ["s2"] = ("StarCraft II", "S2"),
            ["hs_beta"] = ("Hearthstone", "WTCG"),
            ["hero"] = ("Heroes of the Storm", "Hero"),
            ["rtro"] = ("Blizzard Arcade Collection", "RTRO"),
            ["viper"] = ("Call of Duty: Black Ops 4", "VIPR"),
            ["odin"] = ("Call of Duty: Modern Warfare", "ODIN"),
            ["zeus"] = ("Call of Duty: Black Ops Cold War", "ZEUS"),
            ["fore"] = ("Call of Duty: Vanguard", "FORE"),
            ["auks"] = ("Call of Duty", "AUKS"),
            ["lazr"] = ("Call of Duty: Modern Warfare 2 Campaign Remastered", "LAZR"),
            ["wlby"] = ("Crash Bandicoot 4: It's About Time", "WLBY"),
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
                : (KnownGames.TryGetValue(product.ProductCode, out var known) ? known.Name : null) ?? Path.GetFileName(product.InstallPath.TrimEnd('/', '\\'));

            return new Game(
                title: title,
                platform: PlatformName,
                runTask: GetGameRunTask(product, clientExe),
                iconPath: uninstallProgram?.DisplayIconPath ?? clientExe,
                uninstallAction: GetUninstallAction(uninstallProgram, product.Uid));
        }

        private static UninstallProgram? FindUninstallProgram(IEnumerable<UninstallProgram> programs, string uid)
        {
            // Trailing space so "--uid=w3" doesn't match "--uid=w3_legacy"
            return programs.FirstOrDefault(x => x.UninstallCommand is not null && (x.UninstallCommand + " ").Contains($"--uid={uid} "));
        }

        private Func<Task> GetGameRunTask(BattleNetProduct product, string? clientExe)
        {
            var launchId = KnownGames.TryGetValue(product.ProductCode, out var known) ? known.LaunchId : null;

            // battlenet://game/<code> only selects the game in the client, --exec="launch <family id>" actually starts it
            return () =>
            {
                if (clientExe is null || launchId is null)
                {
                    publicApi.OpenAppUri(new Uri($"battlenet://game/{product.ProductCode}"));
                    return Task.CompletedTask;
                }

                var args = $"--exec=\"launch {launchId}\"";
                var clientWasRunning = IsClientRunning(requireWindow: false);
                publicApi.ShellRun(args, clientExe);

                // A cold-started client only selects the game before it logs in, so repeat the launch once its window is up.
                // Runs in background so Flow Launcher can close right away.
                if (!clientWasRunning)
                    _ = Task.Run(async () =>
                    {
                        for (var i = 0; i < 60 && !IsClientRunning(requireWindow: true); i++)
                            await Task.Delay(TimeSpan.FromSeconds(1));

                        await Task.Delay(TimeSpan.FromSeconds(3));
                        publicApi.ShellRun(args, clientExe);
                    });

                return Task.CompletedTask;
            };
        }

        private static bool IsClientRunning(bool requireWindow)
        {
            var processes = Process.GetProcessesByName("Battle.net");
            try
            {
                return processes.Any(x => !requireWindow || x.MainWindowHandle != IntPtr.Zero);
            }
            finally
            {
                foreach (var process in processes)
                    process.Dispose();
            }
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
