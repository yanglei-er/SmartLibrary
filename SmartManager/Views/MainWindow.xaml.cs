using SamrtManager.ViewModels;
using Shared.Helpers;
using Shared.Services.Contracts;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace SamrtManager.Views
{
    public partial class MainWindow : IWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(MainWindowViewModel viewModel,
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService)
        {
            LoadingSettings();

            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            RootNavigation.SetServiceProvider(serviceProvider);
            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
            contentDialogService.SetDialogHost(RootContentDialog);
        }

        private static void LoadingSettings()
        {
            ApplicationTheme theme = Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme"));
            ApplicationThemeManager.Apply(theme, Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
            if (Convert.ToBoolean(SettingsHelper.GetConfig("IsCustomizedAccentColor")))
            {
                ApplicationAccentColorManager.Apply(Utils.StringToColor(SettingsHelper.GetConfig("CustomizedAccentColor")), theme);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
