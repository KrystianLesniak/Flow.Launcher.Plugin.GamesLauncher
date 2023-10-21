using Flow.Launcher.Plugin;
using GamesLauncher.Common;
using GamesLauncher.Common.Settings;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GamesLauncher.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly MainSettings _settings;
        private readonly HidenGames _hidenGames;

        private readonly IPublicAPI _publicAPI;

        public SettingsView(MainSettings settings, HidenGames hidenGames, IPublicAPI publicAPI)
        {
            InitializeComponent();
            _settings = settings;
            _hidenGames = hidenGames;
            _publicAPI = publicAPI;

            HidenGames.ItemsSource = new ObservableCollection<HidenGame>(_hidenGames.Items);
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

        private void BtnOpenCustomShortcutsDirectory_Click(object sender, RoutedEventArgs e)
        {
            _publicAPI.ShellRun(Paths.CustomShortcutsDirectory, "explorer.exe");
        }

        private void BtnShowHiddenGames_Click(object sender, RoutedEventArgs e)
        {
            HidenGamesStackPanel.Visibility = HidenGamesStackPanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnUnhideGame_Click(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not HidenGame gameToUnhide)
                return;

            _hidenGames.Unhide(gameToUnhide.InternalGameId);
            HidenGames.ItemsSource = new ObservableCollection<HidenGame>(_hidenGames.Items);
        }
    }
}
