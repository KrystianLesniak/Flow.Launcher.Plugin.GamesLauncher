using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models;
using Microsoft.Win32;

namespace GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall
{
    internal static class ControlPanelUninstall
    {
        internal static Task<IEnumerable<UninstallProgram>> GetPrograms() => Task.Run<IEnumerable<UninstallProgram>>(() =>
        {
            var programs = new List<UninstallProgram>();

            SearchPrograms(RegistryHive.LocalMachine, RegistryView.Registry32, programs);
            SearchPrograms(RegistryHive.LocalMachine, RegistryView.Registry64, programs);
            SearchPrograms(RegistryHive.CurrentUser, RegistryView.Default, programs);

            return programs;


            static void SearchPrograms(RegistryHive hive, RegistryView view, List<UninstallProgram> programs)
            {
                const string uninstallRootKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";

                using var baseKey = RegistryKey.OpenBaseKey(hive, view);

                using var uninstallSubKey = baseKey.OpenSubKey(uninstallRootKey);
                if (uninstallSubKey == null)
                    return;

                foreach (var programUninstallSubKeyName in uninstallSubKey.GetSubKeyNames())
                {
                    try
                    {
                        using var prog = baseKey.OpenSubKey(uninstallRootKey + programUninstallSubKeyName);
                        if (prog == null)
                            continue;

                        programs.Add(new UninstallProgram()
                        {
                            DisplayIconPath = prog.GetValue("DisplayIcon")?.ToString(),
                            DisplayName = prog.GetValue("DisplayName")?.ToString() ?? string.Empty,
                            UninstallCommand = prog.GetValue("UninstallString")?.ToString(),
                            InstallLocation = prog.GetValue("InstallLocation")?.ToString() ?? string.Empty,
                            SubKeyName = programUninstallSubKeyName
                        });
                    }
                    catch (System.Security.SecurityException)
                    {
                    }
                }
            }
        });
    }
}
