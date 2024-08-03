using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Shared.Models;
using SmartManager.Helpers;
using System.Windows.Media.Imaging;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartManager.ViewModels
{
    public partial class EditFaceViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly Database FacesDb = Database.GetDatabase("faces.smartmanager");
        private bool _initial = true;

        [ObservableProperty]
        private string _uID = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string? _sex = string.Empty;

        [ObservableProperty]
        private string? _age = string.Empty;

        [ObservableProperty]
        private string? _joinTime = string.Empty;

        [ObservableProperty]
        private string _feature = string.Empty;

        [ObservableProperty]
        private BitmapImage _faceImage = new();

        [ObservableProperty]
        public bool _isEditButtonEnabled = false;

        public EditFaceViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            WeakReferenceMessenger.Default.Register<string, string>(this, "EditFace", OnMessageReceived);
        }

        private async void OnMessageReceived(object recipient, string message)
        {
            User user = await FacesDb.GetOneUserAsync(message);
            UID = user.Uid;
            Name = user.Name;
            Sex = user.Sex;
            Age = user.Age;
            JoinTime = user.JoinTime;
            Feature = user.Feature;
            FaceImage = user.FaceImage;
            WeakReferenceMessenger.Default.Unregister<string>(this);
            _initial = false;
        }

        [RelayCommand]
        private async Task EditFaceButtonClick()
        {
            if (string.IsNullOrEmpty(Name))
            {
                System.Media.SystemSounds.Asterisk.Play();
                await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "编辑信息",
                    Content = "您必须完善以下书籍信息， 才能更改用户信息：\n\n姓名不能为空！",
                    CloseButtonText = "去完善",
                });
            }
            else
            {
                FacesDb.UpdateFaceAsync(new(UID, Name, Sex, Age, JoinTime, Feature, FaceImage));

                System.Media.SystemSounds.Asterisk.Play();
                _snackbarService.Show("更改成功", $"用户 {Name} 信息已更改。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                WeakReferenceMessenger.Default.Send("refresh", "FaceManage");
                IsEditButtonEnabled = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.GoBack();
        }

        partial void OnAgeChanged(string? value)
        {
            if (!_initial)
            {
                IsEditButtonEnabled = true;
            }
        }

        partial void OnJoinTimeChanged(string? value)
        {
            if (!_initial)
            {
                IsEditButtonEnabled = true;
            }
        }
    }
}
