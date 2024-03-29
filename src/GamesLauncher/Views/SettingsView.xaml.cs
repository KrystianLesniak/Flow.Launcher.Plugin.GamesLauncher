﻿using Flow.Launcher.Plugin;
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
        private readonly HiddenGames _hiddenGames;

        private readonly IPublicAPI _publicAPI;

        public SettingsView(MainSettings settings, HiddenGames hiddenGames, IPublicAPI publicAPI)
        {
            InitializeComponent();
            _settings = settings;
            _hiddenGames = hiddenGames;
            _publicAPI = publicAPI;

            HiddenGames.ItemsSource = new ObservableCollection<HiddenGame>(_hiddenGames.Items);
        }

        private void SettingsView_OnLoaded(object sender, RoutedEventArgs re)
        {
            SynchronizeSteam.IsChecked = _settings.SynchronizeSteam;
            SyncrhonizeEpic.IsChecked = _settings.SynchronizeEpicGamesStore;
            SynchronizeXbox.IsChecked = _settings.SynchronizeXbox;
            SynchronizeGogGalaxy.IsChecked = _settings.SynchronizeGogGalaxy;
            SynchronizeEaApp.IsChecked = _settings.SynchronizeEaApp;
            SynchronizeUbisoft.IsChecked = _settings.SynchronizeUbisoft;
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

            SynchronizeGogGalaxy.Checked += (o, e) =>
            {
                _settings.SynchronizeGogGalaxy = true;
            };
            SynchronizeGogGalaxy.Unchecked += (o, e) =>
            {
                _settings.SynchronizeGogGalaxy = false;
            };

            SynchronizeEaApp.Checked += (o, e) =>
            {
                _settings.SynchronizeEaApp = true;
            };
            SynchronizeEaApp.Unchecked += (o, e) =>
            {
                _settings.SynchronizeEaApp = false;
            };

            SynchronizeUbisoft.Checked += (o, e) =>
            {
                _settings.SynchronizeUbisoft = true;
            };
            SynchronizeUbisoft.Unchecked += (o, e) =>
            {
                _settings.SynchronizeUbisoft = false;
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
            HiddenGamesStackPanel.Visibility = HiddenGamesStackPanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnUnhideGame_Click(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not HiddenGame gameToUnhide)
                return;

            _hiddenGames.Unhide(gameToUnhide);
            HiddenGames.ItemsSource = new ObservableCollection<HiddenGame>(_hiddenGames.Items);
        }
    }
}
