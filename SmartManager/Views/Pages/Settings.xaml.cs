﻿using SamrtManager.ViewModels;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Wpf.Ui.Controls;
namespace SamrtManager.Views.Pages
{
    public partial class Settings : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }
        private static readonly List<string> SystemAccentColor = ["#FFB900", "#FF8C00", "#F7630C", "#CA5010", "#DA3B01", "#EF6950", "#D13438", "#FF4343", "#E74856", "#E81123", "#EA005E", "#C30052", "#E3008C", "#BF0077", "#C239B3", "#9A0089", "#0078D4", "#0063B1", "#8E8CD8", "#6B69D6", "#8764B8", "#744DA9", "#B146C2", "#881798", "#0099BC", "#2D7D96", "#00B7C3", "#038387", "#00B294", "#018574", "#00CC6A", "#10893E", "#7A7574", "#5D5A58", "#68768A", "#515C6B", "#567C73", "#486860", "#498205", "#107C10", "#767676", "#4C4A48", "#69797E", "#4A5459", "#647C64", "#525E54", "#847545", "#7E735F"];

        public Settings(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            AccentColor.ItemsSource = SystemAccentColor;

            AppVersion.Text = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            DotNetVersion.Content = ".Net " + Environment.Version.ToString();
            WpfUIVersion.Content = "WPF-UI " + (FileVersionInfo.GetVersionInfo("./Wpf.Ui.dll").ProductVersion ?? string.Empty).Split("+")[0];
        }

        private void ColorExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ViewModel.ColorExpander_Expanded();
        }
    }
}