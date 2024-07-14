using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SmartManager.Helpers;
using Wpf.Ui;

namespace SmartManager.ViewModels
{
    public partial class EditFaceViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly Database FacesDb = Database.GetDatabase("faces.smartmanager");

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        public bool _isEditButtonEnabled = false;

        public EditFaceViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            WeakReferenceMessenger.Default.Register<string, string>(this, "EditFace", OnMessageReceived);
        }

        private void OnMessageReceived(object recipient, string message)
        {
            Name = message;
            WeakReferenceMessenger.Default.Unregister<string>(this);
        }
    }
}
