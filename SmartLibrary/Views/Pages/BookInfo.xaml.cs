using SmartLibrary.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class BookInfo : INavigableView<BookInfoViewModel>
    {
        public BookInfoViewModel ViewModel { get; }

        public BookInfo(BookInfoViewModel viewModel)
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
                    if (string.IsNullOrEmpty(isbnBox.Text))
                    {
                        XuNiBox.Focus();
                    }
                    else if (isbnBox.Text.Length == 13)
                    {
                        XuNiBox.Focus();
                        ViewModel.OnSearchButtonClick();
                    }
                }
            }
        }
    }
}