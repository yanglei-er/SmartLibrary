using SmartLibrary.ViewModels;
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
    }
}
