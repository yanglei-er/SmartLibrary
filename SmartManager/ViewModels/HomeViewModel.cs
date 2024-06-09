using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SamrtManager.Views.Pages;
using Wpf.Ui;

namespace SamrtManager.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void OnCardClick(string parameter)
        {
            if (parameter == "Face")
            {
                _navigationService.Navigate(typeof(FaceManage));
            }
            else if (parameter == "RFID")
            {
                _navigationService.Navigate(typeof(RFIDManage));
            }
            else
            {
                _navigationService.Navigate(typeof(Settings));
            }
        }
    }
}
