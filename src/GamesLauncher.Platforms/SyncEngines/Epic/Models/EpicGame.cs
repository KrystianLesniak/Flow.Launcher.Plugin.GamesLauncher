using Newtonsoft.Json.Linq;

namespace GamesLauncher.Platforms.SyncEngines.Epic.Models
{
    internal class EpicGame
    {
        public required string DisplayName { get; set; }
        public required string CatalogNamespace { get; set; }
        public required string CatalogItemId { get; set; }
        public required string AppName { get; set; }
        public required string? InstallLocation { get; set; }
        public required string? LaunchExecutable { get; set; }

        internal static EpicGame? CreateFromJObject(JObject? jObject)
        {
            if (jObject == null)
                return null;

            var displayName = jObject.Value<string?>(nameof(DisplayName));
            var catalogNamespace = jObject.Value<string?>(nameof(CatalogNamespace));
            var catalogItemId = jObject.Value<string?>(nameof(CatalogItemId));
            var appName = jObject.Value<string?>(nameof(AppName));
            var installLocation = jObject.Value<string?>(nameof(InstallLocation));
            var launchExecutable = jObject.Value<string?>(nameof(LaunchExecutable));

            if (displayName == null || catalogNamespace == null || catalogItemId == null || appName == null)
                return null;

            if (jObject.Value<string?>("MainGameCatalogItemId") != catalogItemId) // If this is an addon/DLC mainGameCatalogItemId and catalogItemId will be different
                return null;

            if (jObject.Value<bool?>("bIsIncompleteInstall") == true) // If game installation is not completed this flag is true
                return null;

            return new EpicGame
            {
                DisplayName = displayName,
                CatalogNamespace = catalogNamespace,
                CatalogItemId = catalogItemId,
                AppName = appName,
                InstallLocation = installLocation,
                LaunchExecutable = launchExecutable,
            };
        }
    }
}
