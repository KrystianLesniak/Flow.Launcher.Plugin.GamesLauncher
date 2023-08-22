using System.Text.Json.Nodes;

namespace Flow.Launcher.Plugin.GamesLauncher.SyncEngines.EpicSyncEngine.Models
{
    internal class EpicGame
    {
        public required string DisplayName { get; set; }
        public required string CatalogNamespace { get; set; }
        public required string CatalogItemId { get; set; }
        public required string AppName { get; set; }
        public required string? InstallLocation { get; set; }
        public required string? LaunchExecutable { get; set; }

        internal static EpicGame? CreateFromJsonNode(JsonNode? node)
        {
            if (node == null)
                return null;

            var displayName = node[nameof(DisplayName)];
            var catalogNamespace = node[nameof(CatalogNamespace)];
            var catalogItemId = node[nameof(CatalogItemId)];
            var appName = node[nameof(AppName)];
            var installLocation = node[nameof(InstallLocation)];
            var launchExecutable = node[nameof(LaunchExecutable)];

            if (displayName == null || catalogNamespace == null || catalogItemId == null || appName == null)
                return null;

            return new EpicGame
            {
                DisplayName = displayName.GetValue<string>(),
                CatalogNamespace = catalogNamespace.GetValue<string>(),
                CatalogItemId = catalogItemId.GetValue<string>(),
                AppName = appName.GetValue<string>(),
                InstallLocation = installLocation?.GetValue<string>(),
                LaunchExecutable = launchExecutable?.GetValue<string>(),
            };
        }
    }
}
