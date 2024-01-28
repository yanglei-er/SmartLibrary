using Wpf.Ui;

namespace SmartLibrary.ViewModels
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
            if (String.IsNullOrWhiteSpace(parameter))
            {
                return;
            }

            if(parameter == "我要找书")
            {
                _navigationService.Navigate(typeof(Views.Pages.Bookshelf));
            }
            else if(parameter == "我要借/还书")
            {
                _navigationService.Navigate(typeof(Views.Pages.Borrow_Return_Book));
            }
            else if(parameter == "图书信息查询")
            {
                _navigationService.Navigate(typeof(Views.Pages.BookInfo));
            }
            else if(parameter == "蓝牙")
            {
                _navigationService.Navigate(typeof(Views.Pages.BluetoothSettings));
            }
            else if( parameter == "设置")
            {
                _navigationService.Navigate(typeof(Views.Pages.Settings));
            }
        }
    }
}
