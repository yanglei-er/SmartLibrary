﻿using SmartLibrary.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Abstractions.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class EditBook : INavigableView<EditBookViewModel>
    {
        public EditBookViewModel ViewModel { get; }

        public EditBook(EditBookViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                XuNiBox.Focus();
            }
            else
            {
                ViewModel.IsEditButtonEnabled = true;
            }
        }
    }
}