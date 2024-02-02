using SmartLibrary.ViewModels;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class Home : INavigableView<HomeViewModel>
    {
        public HomeViewModel ViewModel { get; }
        public Home(HomeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
