using SmartLibrary.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class Bookshelf : INavigableView<BookshelfViewModel>
    {
        public BookshelfViewModel ViewModel { get; }

        public Bookshelf(BookshelfViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.GotoTargetPage(textBox.Text);
                XuNiBox.Focus();
            }
        }
    }
}