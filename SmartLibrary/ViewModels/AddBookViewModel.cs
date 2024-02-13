using Wpf.Ui;

namespace SmartLibrary.ViewModels
{
    public partial class AddBookViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        public AddBookViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.GoBack();
        }
    }
}
