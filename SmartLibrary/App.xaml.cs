using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Helpers;
using Shared.Services;
using Shared.Services.Contracts;
using SmartLibrary.Services;
using System.Diagnostics;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;
namespace SmartLibrary
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
                    services.AddNavigationViewPageProvider();

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
                    services.AddSingleton<Views.Pages.Bookshelf>();
                    services.AddSingleton<ViewModels.BookshelfViewModel>();
                    services.AddSingleton<Views.Pages.Borrow_Return_Book>();
                    services.AddSingleton<ViewModels.Borrow_Return_BookViewModel>();
                    services.AddSingleton<Views.Pages.BookInfo>();
                    services.AddSingleton<ViewModels.BookInfoViewModel>();
                    services.AddSingleton<Views.Pages.BookManage>();
                    services.AddSingleton<ViewModels.BookManageViewModel>();
                    services.AddSingleton<Views.Pages.BluetoothSettings>();
                    services.AddSingleton<ViewModels.BluetoothSettingsViewModel>();
                    services.AddSingleton<Views.Pages.Settings>();
                    services.AddSingleton<ViewModels.SettingsViewModel>();

                    services.AddTransient<Views.Pages.AddBook>();
                    services.AddTransient<ViewModels.AddBookViewModel>();
                    services.AddTransient<Views.Pages.EditBook>();
                    services.AddTransient<ViewModels.EditBookViewModel>();
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
            mutex = new Mutex(true, "SmartLibrary", out bool aIsNewInstance);
            if (aIsNewInstance)
            {
                _host.Start();
                mutex.ReleaseMutex();
            }
            else
            {
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        Win32Helper.SendMessageString(process.MainWindowHandle, "SmartLibrary");
                        break;
                    }
                }
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