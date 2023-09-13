namespace GamesLauncher.Common
{
    public static class Paths
    {
        public static string CustomShortcutsDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FlowLauncher", "Settings", "Plugins", "GamesLauncher Custom Shortcuts");
    }
}
