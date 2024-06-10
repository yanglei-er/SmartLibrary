using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SamrtManager.Services;
using Shared.Helpers;
using Shared.Services;
using Shared.Services.Contracts;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui;

namespace SamrtManager
{
    public partial class App : Application
    {
        private Mutex? mutex;

        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(AppContext.BaseDirectory);
            })
            .ConfigureServices(
                (context, services) =>
                {
                    // App Host
                    services.AddHostedService<ApplicationHostService>();

                    // Main window container with navigation
                    services.AddSingleton<IWindow, Views.MainWindow>();
                    services.AddSingleton<ViewModels.MainWindowViewModel>();
                    services.AddSingleton<IContentDialogService, ContentDialogService>();
                    services.AddSingleton<ISnackbarService, SnackbarService>();
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<WindowsProviderService>();

                    // Views and ViewModels
                    services.AddSingleton<Views.Pages.Home>();
                    services.AddSingleton<ViewModels.HomeViewModel>();
                    services.AddSingleton<Views.Pages.FaceManage>();
                    services.AddSingleton<ViewModels.FaceManageViewModel>();
                    services.AddSingleton<Views.Pages.RFIDManage>();
                    services.AddSingleton<ViewModels.RFIDManageViewModel>();

                    services.AddSingleton<Views.Pages.Settings>();
                    services.AddSingleton<ViewModels.SettingsViewModel>();
                }
            ).Build();

        public static T? GetService<T>()
        where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        public static T GetRequiredService<T>()
        where T : class
        {
            return _host.Services.GetRequiredService<T>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            mutex = new Mutex(true, "SmartManage", out bool aIsNewInstance);
            if (aIsNewInstance)
            {
                _host.Start();
                mutex.ReleaseMutex();
            }
            else
            {
                NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                Current.Shutdown();
            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            _host.StopAsync().Wait();
            _host.Dispose();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题...\n " + e.Exception.Message);
            e.Handled = true;
        }
    }
}
