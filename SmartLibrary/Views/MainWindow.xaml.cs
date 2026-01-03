using Shared.Helpers;
using Shared.Services.Contracts;
using SmartLibrary.Helpers;
using SmartLibrary.ViewModels;
using System.Runtime.InteropServices;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace SmartLibrary.Views
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
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            WindowBackdropType = Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop"));

            RootNavigation.SetServiceProvider(serviceProvider);
            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
            contentDialogService.SetDialogHost(RootContentDialog);

            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingSettings();
#if RELEASE
            WindowInteropHelper helper = new(this);
            HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(new HwndSourceHook(WndProc));
#endif
        }

        private void LoadingSettings()
        {
            ApplicationTheme theme = Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme"));
            if (SettingsHelper.GetConfig("Theme") == "System")
            {
                SystemThemeWatcher.Watch(this);
                ApplicationThemeManager.Changed += (t, _) => { ResourceManager.UpdateTheme(Utils.GetUserApplicationTheme(t.ToString()).ToString()); };
            }
            ResourceManager.UpdateTheme(theme.ToString());
            ApplicationThemeManager.Apply(theme);
            if (SettingsHelper.GetBoolean("IsCustomizedAccentColor"))
            {
                ApplicationAccentColorManager.Apply(Utils.StringToColor(SettingsHelper.GetConfig("CustomizedAccentColor")), theme);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == Win32Helper.WM_COPYDATA)
            {
                object? o = Marshal.PtrToStructure<Win32Helper.COPYDATASTRUCT>(lparam);
                if (o != null)
                {
                    Win32Helper.COPYDATASTRUCT cds = (Win32Helper.COPYDATASTRUCT)o;
                    string? receivedMessage = Marshal.PtrToStringUni(cds.lpData);
                    if (receivedMessage == "SmartLibrary")
                    {
                        if (WindowState == WindowState.Minimized || Visibility != Visibility.Visible)
                        {
                            Show();
                            WindowState = WindowState.Normal;
                        }
                        Activate();
                        Topmost = true;
                        Topmost = false;
                        Focus();
                    }
                }
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            FaceRecognition.CloseCamera();
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}