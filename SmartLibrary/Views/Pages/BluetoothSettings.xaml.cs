﻿using SmartLibrary.ViewModels;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class BluetoothSettings : INavigableView<BluetoothSettingsViewModel>
    {
        public BluetoothSettingsViewModel ViewModel { get; }
        public BluetoothSettings(BluetoothSettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.OnListViewSelecteChanged();
        }
    }
}