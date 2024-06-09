using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SamrtManager.Views;
using SamrtManager.Views.Pages;
using Shared.Services.Contracts;
using System.Windows;

namespace SamrtManager.Services
{
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationHostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return HandleActivationAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private Task HandleActivationAsync()
        {
            if (Application.Current.Windows.OfType<MainWindow>().Any())
            {
                return Task.CompletedTask;
            }

            IWindow mainWindow = _serviceProvider.GetRequiredService<IWindow>();

            mainWindow.Loaded += (sender, _) => { if (sender is MainWindow mainWindow) { mainWindow.RootNavigation.Navigate(typeof(Home)); } };
            mainWindow?.Show();
            return Task.CompletedTask;
        }
    }
}
