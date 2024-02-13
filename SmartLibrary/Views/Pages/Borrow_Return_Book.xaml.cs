using SmartLibrary.ViewModels;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class Borrow_Return_Book : INavigableView<Borrow_Return_BookViewModel>
    {
        public Borrow_Return_BookViewModel ViewModel { get; }
        public Borrow_Return_Book(Borrow_Return_BookViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
