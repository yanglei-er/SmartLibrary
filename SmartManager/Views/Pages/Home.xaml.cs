using SamrtManager.ViewModels;
using Wpf.Ui.Controls;

namespace SamrtManager.Views.Pages
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
