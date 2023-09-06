using GamesLauncher.Common.Settings;
using System.Windows;
using System.Windows.Controls;

namespace GamesLauncher.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly MainSettings _settings;

        public SettingsView(MainSettings settings)
        {
            InitializeComponent();
            _settings = settings;
        }

        private void SettingsView_OnLoaded(object sender, RoutedEventArgs re)
        {
            SynchronizeSteam.IsChecked = _settings.SynchronizeSteam;
            SyncrhonizeEpic.IsChecked = _settings.SynchronizeEpicGamesStore;
            SynchronizeXbox.IsChecked = _settings.SynchronizeXbox;
            SynchronizeAmazon.IsChecked = _settings.SynchronizeAmazon;

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

            SynchronizeXbox.Checked += (o, e) =>
            {
                _settings.SynchronizeXbox = true;
            };
            SynchronizeXbox.Unchecked += (o, e) =>
            {
                _settings.SynchronizeXbox = false;
            };

            SynchronizeAmazon.Checked += (o, e) =>
            {
                _settings.SynchronizeAmazon = true;
            };
            SynchronizeAmazon.Unchecked += (o, e) =>
            {
                _settings.SynchronizeAmazon = false;
            };
        }
    }
}
