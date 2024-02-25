using SmartLibrary.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class AddBook : INavigableView<AddBookViewModel>
    {
        public AddBookViewModel ViewModel { get; }
        public AddBook(AddBookViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void IsbnBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox isbnBox)
                {
                    if (isbnBox.Text.Length == 13)
                    {
                        XuNiBox.Focus();
                        ViewModel.OnSearchButtonClick(isbnBox.Text);
                    }
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                XuNiBox.Focus();
            }
        }
    }
}
