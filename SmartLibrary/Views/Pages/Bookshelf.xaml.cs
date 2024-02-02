using SmartLibrary.ViewModels;
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
    }
}
