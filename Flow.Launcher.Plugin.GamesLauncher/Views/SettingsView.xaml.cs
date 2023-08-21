using Flow.Launcher.Plugin.GamesLauncher;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.GamesLauncher.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly Settings _settings;

        public SettingsView(Settings settings)
        {
            InitializeComponent();
            _settings = settings;
        }

        private void SettingsView_OnLoaded(object sender, RoutedEventArgs re)
        {
            SynchronizeSteam.IsChecked = _settings.SynchronizeSteam;    
            SyncrhonizeEpic.IsChecked = _settings.SynchronizeEpicGamesStore;

            SynchronizeSteam.Checked += (o, e) =>
            {
                _settings.SynchronizeSteam = true;
            };
            SynchronizeSteam.Unchecked += (o, e) =>
            {
                _settings.SynchronizeSteam = false;
            };

            SyncrhonizeEpic.Checked += (o, e) =>
            {
                _settings.SynchronizeEpicGamesStore = true;
            };
            SyncrhonizeEpic.Unchecked += (o, e) =>
            {
                _settings.SynchronizeEpicGamesStore = false;
            };

        }
    }
}
