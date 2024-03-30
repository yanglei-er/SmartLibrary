using Microsoft.Extensions.DependencyInjection;

namespace SmartLibrary.Services
{
    public class WindowsProviderService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowsProviderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Show<T>()
            where T : class
        {
            if (!typeof(Window).IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException($"The window class should be derived from {typeof(Window)}.");
            }

            Window windowInstance = _serviceProvider.GetService<T>() as Window ?? throw new InvalidOperationException("Window is not registered as service.");
            windowInstance.Owner = Application.Current.MainWindow;
            windowInstance.Show();
        }
    }

}
