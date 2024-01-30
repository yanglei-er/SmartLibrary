using SmartLibrary.Helpers;
using System.Windows.Interop;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views
{
    public partial class MainWindow : INavigationWindow
    {
        public ViewModels.MainWindowViewModel ViewModel { get; }

        public MainWindow(
            ViewModels.MainWindowViewModel viewModel,
            IPageService pageService,
            INavigationService navigationService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            InitializeComponent();

            SetPageService(pageService);
            navigationService.SetNavigationControl(RootNavigation);

            this.Loaded += Window_Loaded;
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
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(new HwndSourceHook(BluetoothHelper.Instance.HwndHandler));  //挂钩
        }

        //void OnBleStateChanged(bool state)
        //{
        //    ToastContentBuilder toast = new ToastContentBuilder();
        //    toast.AddArgument("action", "viewConversation");
        //    toast.AddArgument("conversationId", 9813);
        //    if (state)
        //    {
        //        toast.AddText("系统蓝牙已开启！", hintMaxLines: 1);
        //        toast.AddText("您可以执行下一步操作");
        //        toast.AddAppLogoOverride(new Uri("pack://application:,,,/Assets/bluetooth.png"));
        //    }
        //    else
        //    {
        //        toast.AddText("系统蓝牙已关闭！", hintMaxLines: 1);
        //        toast.AddText("为正常使用程序全部功能，请开启系统蓝牙");
        //        toast.AddButton(new ToastButton().SetContent("转到蓝牙设置").AddArgument("action", "toBleSettings"));
        //        toast.AddAppLogoOverride(new Uri("pack://application:,,,/Assets/bluetooth-disabled.png"));
        //    }
        //    toast.Show();
        //}

        protected override void OnClosed(EventArgs e)
        {
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
