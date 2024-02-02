using SmartLibrary.ViewModels;
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
    }
}
