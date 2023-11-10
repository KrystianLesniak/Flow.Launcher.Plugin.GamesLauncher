using GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall.Models;
using Microsoft.Win32;
using System.Collections.Concurrent;

namespace GamesLauncher.Platforms.SyncEngines.Common.ControlPanelUninstall
{
    internal static class ControlPanelUninstall
    {
        internal static async Task<IEnumerable<UninstallProgram>> GetPrograms()
        {
            var programs = new ConcurrentBag<UninstallProgram>();

            var registryHives = new RegistryHive[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser };
            var registryViews = new RegistryView[] { RegistryView.Registry32, RegistryView.Registry64 };

            await Parallel.ForEachAsync(registryHives, async (hive, ct) =>
            {
                await Parallel.ForEachAsync(registryViews, async (view, ct) =>
                {
                    SearchPrograms(hive, view, programs);
                    await Task.CompletedTask;
                });
            });

            return programs;


            static void SearchPrograms(RegistryHive hive, RegistryView view, ConcurrentBag<UninstallProgram> programs)
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
                            DisplayIcon = prog.GetValue("DisplayIcon")?.ToString() ?? string.Empty,
                            DisplayName = prog.GetValue("DisplayName")?.ToString() ?? string.Empty,
                            SubKeyName = programUninstallSubKeyName
                        });
                    }
                    catch (System.Security.SecurityException)
                    {
                    }
                }
            }
        }
    }
}
