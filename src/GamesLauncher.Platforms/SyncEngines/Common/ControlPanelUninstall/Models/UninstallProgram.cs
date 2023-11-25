namespace GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models
{
    internal class UninstallProgram
    {
        public string DisplayName { get; internal set; } = string.Empty;
        public string SubKeyName { get; internal set; } = string.Empty;

        public string? DisplayIconPath {
            get
            {
                return displayIconPath is null
                    ? displayIconPath
                    : Path.Combine(displayIconPath.Replace("\"", string.Empty).Trim('\\'));
            } 
            internal set => displayIconPath = value; 
        }
        private string? displayIconPath;

        public string? UninstallCommand { get; internal set; }
        public string InstallLocation { get; internal set; } = string.Empty;

    }
}
