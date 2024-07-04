using Shared.Helpers;
using Shared.Services.Contracts;
using SmartManager.Helpers;
using SmartManager.ViewModels;
using System.Windows;
using System.Windows.Interop;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace SmartManager.Views
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

#if RELEASE
            Loaded += Window_Loaded;
#endif
        }

        private static void LoadingSettings()
        {
            ApplicationTheme theme = Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme"));
            ResourceManager.UpdateTheme(theme.ToString());
            ApplicationThemeManager.Apply(theme, Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
            if (Convert.ToBoolean(SettingsHelper.GetConfig("IsCustomizedAccentColor")))
            {
                ApplicationAccentColorManager.Apply(Utils.StringToColor(SettingsHelper.GetConfig("CustomizedAccentColor")), theme);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new(this);
            HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == NativeMethods.WM_SHOWME)
            {
                if (WindowState == WindowState.Minimized || Visibility != Visibility.Visible)
                {
                    Show();
                    WindowState = WindowState.Normal;
                }

                // According to some sources these steps gurantee that an app will be brought to foreground.
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
