using SmartLibrary.Helpers;
using System.Windows.Interop;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views
{
    public partial class MainWindow : INavigationWindow
    {
        private readonly ISnackbarService _snackbarService;
        public ViewModels.MainWindowViewModel ViewModel { get; }

        public MainWindow(ViewModels.MainWindowViewModel viewModel, IPageService pageService, INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            ViewModel = viewModel;
            DataContext = this;

            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            InitializeComponent();

            SetPageService(pageService);
            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
            contentDialogService.SetContentPresenter(RootContentDialog);

            _snackbarService = snackbarService;
#if RELEASE
            this.Loaded += Window_Loaded;
#endif
            BluetoothHelper.Instance.BleStateChangedEvent += OnBleStateChanged;
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new(this);
            HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(new HwndSourceHook(BluetoothHelper.Instance.HwndHandler));  //挂钩
        }

        void OnBleStateChanged(bool state)
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
            BluetoothHelper.Instance.BleStateChangedEvent -= OnBleStateChanged;
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
