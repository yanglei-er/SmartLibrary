﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartLibrary.Models;
using SmartLibrary.Services;
using System.Windows.Threading;
using Wpf.Ui;

namespace SmartLibrary
{
    public partial class App : Application
    {
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

                    // Page resolver service
                    services.AddSingleton<IPageService, PageService>();

                    // ContentDialog manipulation
                    services.AddSingleton<IContentDialogService, ContentDialogService>();

                    // SnackBar manipulation
                    services.AddSingleton<ISnackbarService, SnackbarService>();

                    // Service containing navigation, same as INavigationWindow... but without window
                    services.AddSingleton<INavigationService, NavigationService>();

                    // Main window with navigation
                    services.AddSingleton<INavigationWindow, Views.MainWindow>();
                    services.AddSingleton<ViewModels.MainWindowViewModel>();

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

                    // Configuration
                    services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
                }
            ).Build();

        public static T? GetService<T>()
        where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题...\n " + e.Exception.Message);
            e.Handled = true;
        }
    }
}