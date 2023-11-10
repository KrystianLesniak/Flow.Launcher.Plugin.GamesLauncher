namespace GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models
{
    internal class UninstallProgram
    {
        public string DisplayName { get; internal set; } = string.Empty;
        public string SubKeyName { get; internal set; } = string.Empty;
        public string? DisplayIcon { get; internal set; }
        public string? UninstallCommand { get; internal set; }
    }
}
