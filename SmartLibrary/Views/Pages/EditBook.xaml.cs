using SmartLibrary.ViewModels;
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
    }
}