using System.Windows;

namespace Shared.Services.Contracts
{
    public interface IWindow
    {
        event RoutedEventHandler Loaded;
        void Show();
    }
}
