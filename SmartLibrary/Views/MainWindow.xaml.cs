using SmartLibrary.Helpers;
using SmartLibrary.Services.Contracts;
using SmartLibrary.ViewModels;
using System.Windows.Interop;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views
{
    public partial class MainWindow : IWindow
    {
        private readonly ISnackbarService _snackbarService;
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

            _snackbarService = snackbarService;
#if RELEASE
            Loaded += Window_Loaded;
#endif
            BluetoothHelper.BleStateChangedEvent += OnBleStateChanged;
        }

        private void LoadingSettings()
        {
            ApplicationTheme theme = Helpers.Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme"));
            ApplicationThemeManager.Apply(theme);
            if (Convert.ToBoolean(SettingsHelper.GetConfig("IsCustomizedAccentColor")))
            {
                ApplicationAccentColorManager.Apply(Helpers.Utils.StringToColor(SettingsHelper.GetConfig("CustomizedAccentColor")), theme);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new(this);
            HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(new HwndSourceHook(BluetoothHelper.HwndHandler));
        }

        private void OnBleStateChanged(bool state)
        {
            System.Media.SystemSounds.Asterisk.Play();
            if (state)
            {
                _snackbarService.Show("系统蓝牙已开启！", "请连接蓝牙设备，稍后您可以正常使用程序的全部功能。", ControlAppearance.Info, new SymbolIcon(SymbolRegular.Bluetooth16), TimeSpan.FromSeconds(5));
            }
            else
            {
                _snackbarService.Show("系统蓝牙已关闭！", "为正常使用程序全部功能，请转到蓝牙设置，并开启系统蓝牙。", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.BluetoothDisabled24), TimeSpan.FromSeconds(5));
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            BluetoothHelper.BleStateChangedEvent -= OnBleStateChanged;
            BluetoothHelper.StartDisconnect();
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}