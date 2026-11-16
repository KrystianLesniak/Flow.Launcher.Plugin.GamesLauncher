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
        private readonly HiddenGames _hiddenGames;

        private readonly IPublicAPI _publicAPI;

        public MainSettings Settings { get; }

        public SettingsView(MainSettings settings, HiddenGames hiddenGames, IPublicAPI publicAPI)
        {
            Settings = settings;
            _hiddenGames = hiddenGames;
            _publicAPI = publicAPI;
            InitializeComponent();

            HiddenGames.ItemsSource = new ObservableCollection<HiddenGame>(_hiddenGames.Items);
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
