using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartLibrary.ViewModels
{
    public partial class EditBookViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        public EditBookViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            WeakReferenceMessenger.Default.Register<string>(this, OnMessageReceived);
        }

        private void OnMessageReceived(object recipient, string message)
        {
            System.Windows.MessageBox.Show(message);
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.GoBack();
        }

        public void OnNavigatedTo() { }
        public void OnNavigatedFrom()
        {
            WeakReferenceMessenger.Default.Unregister<string>(this);
        }
    }
}
