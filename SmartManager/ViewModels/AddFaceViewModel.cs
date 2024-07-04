using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Shared.Helpers;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartManager.ViewModels
{
    public partial class AddFaceViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        private readonly Dictionary<string, int> devices = FaceRecognition.GetSystemCameraDevices();
        private static VideoCapture videoCapture = new();
        private readonly Thread video_thread;
        private Image cameraImage = new();

        [ObservableProperty]
        private List<string> _devicesName = [];

        [ObservableProperty]
        private int _deviceSelectedIndex = -1;

        [ObservableProperty]
        private bool _isOpenCameraButtonEnabled = false;

        [ObservableProperty]
        private bool _isCameraOpened = false;

        public AddFaceViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            DevicesName = [.. devices.Keys];
            if (DevicesName.Count > 0)
            {
                DeviceSelectedIndex = 0;
            }

            video_thread = new(PlayVideo);
        }

        partial void OnDeviceSelectedIndexChanged(int value)
        {
            IsOpenCameraButtonEnabled = true;
        }

        public void OnNavigatedTo()
        {

        }

        public void OnNavigatedFrom()
        {
            if (IsCameraOpened)
            {
                videoCapture.Release();
            }
        }

        public void OpenCamera(Image image)
        {
            if (!IsCameraOpened)
            {
                IsOpenCameraButtonEnabled= false;
                cameraImage = image;
                video_thread.Start();
            }
            else
            {
                IsCameraOpened = false;
                image.Source = null;
                videoCapture.Release();
            }
        }

        private void PlayVideo()
        {
            if (devices.TryGetValue(DevicesName[DeviceSelectedIndex], out int videoCapture_id))
            {
                videoCapture = new(videoCapture_id);
                videoCapture.Set(VideoCaptureProperties.FrameWidth, 720);
                videoCapture.Set(VideoCaptureProperties.FrameHeight, 480);//高度
                videoCapture.Set(VideoCaptureProperties.Fps, 12);

                try
                {
                    if (videoCapture.IsOpened())
                    {
                        IsCameraOpened = true;
                        IsOpenCameraButtonEnabled = true;
                    }
                }
                catch (Exception e)
                {
                    _snackbarService.Show("错误", $"开启摄像头失败：{e.Message}", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
                }
            }
            else
            {
                _snackbarService.Show("错误", $"开启摄像头失败", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
            }

            while (IsCameraOpened)
            {
                Mat v_mat = new();
                videoCapture.Read(v_mat);

                int sleepTime = (int)Math.Round(1000 / videoCapture.Fps);

                Cv2.WaitKey(sleepTime);
                if (v_mat.Empty())
                {
                    break;
                }
                cameraImage.Dispatcher.Invoke(new Action(() => { cameraImage.Source = v_mat.ToBitmapSource(); }));
                v_mat.Release();
            }
        }

        [RelayCommand]
        void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
