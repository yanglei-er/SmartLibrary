using Shared.Helpers;
using Shared.Services.Contracts;
using SmartManager.Helpers;
using SmartManager.ViewModels;
using System.Runtime.InteropServices;
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

        private void LoadingSettings()
        {
            ApplicationTheme theme = Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme"));
            if (SettingsHelper.GetConfig("Theme") == "System")
            {
                SystemThemeWatcher.Watch(this);
                ApplicationThemeManager.Changed += (t, _) => { ResourceManager.UpdateTheme(Utils.GetUserApplicationTheme(t.ToString()).ToString()); };
            }
            ResourceManager.UpdateTheme(theme.ToString());
            ApplicationThemeManager.Apply(theme, Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
            if (SettingsHelper.GetBoolean("IsCustomizedAccentColor"))
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
            if (msg == Win32Helper.WM_COPYDATA)
            {
                object? o = Marshal.PtrToStructure(lparam, typeof(Win32Helper.COPYDATASTRUCT));
                if(o != null)
                {
                    Win32Helper.COPYDATASTRUCT cds = (Win32Helper.COPYDATASTRUCT)o;
                    string? receivedMessage = Marshal.PtrToStringUni(cds.lpData);
                    if(receivedMessage == "SmartManager")
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
