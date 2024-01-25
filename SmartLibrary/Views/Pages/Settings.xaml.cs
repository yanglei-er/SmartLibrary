using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class Settings : INavigableView<ViewModels.SettingsViewModel>
    {
        public ViewModels.SettingsViewModel ViewModel { get; }
        public Settings(ViewModels.SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
