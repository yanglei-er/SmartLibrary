using SmartLibrary.Helpers;
using Wpf.Ui;

namespace SmartLibrary.ViewModels
{
    public partial class AddBookViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _isbnBoxPlaceholderText = "请输入13位ISBN码";
        [ObservableProperty]
        private string _isbnBoxText = string.Empty;
        [ObservableProperty]
        private bool _isbnAttitudeVisible = false;
        [ObservableProperty]
        private string _isbnAttitudeImage = string.Empty;
        [ObservableProperty]
        private bool _isScanButtonEnabled = false;
        [ObservableProperty]
        private bool _isSearchButtonEnabled = false;

        public AddBookViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            if (BluetoothHelper.Instance.IsBleConnected)
            {
                IsbnBoxText = "请扫描或输入13位ISBN码";
            }
        }

        partial void OnIsbnBoxTextChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                IsbnAttitudeVisible = false;
                return;
            }
            IsbnAttitudeVisible = true;
            if (value.Length == 13)
            {
                IsbnAttitudeImage = "pack://application:,,,/Assets/pic/right.png";
                IsSearchButtonEnabled = true;
            }
            else
            {
                IsbnAttitudeImage = "pack://application:,,,/Assets/pic/wrong.png";
                IsSearchButtonEnabled = false;
            }
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.GoBack();
        }
    }
}
