using SmartLibrary.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Controls;

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
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ViewModel.IsEditButtonEnabled = true;
        }
    }
}