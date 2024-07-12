using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Helpers;
using SmartManager.Helpers;
using System.Collections.ObjectModel;
using System.Drawing;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartManager.ViewModels
{
    public partial class AddFaceViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        [ObservableProperty]
        private bool _isCameraOpened = false;

        [ObservableProperty]
        private List<string> _devicesName = [];

        [ObservableProperty]
        private int _deviceSelectedIndex = 0;

        [ObservableProperty]
        private bool _isOpenCameraButtonEnabled = false;

        [ObservableProperty]
        private bool _isDrawFaceRectangle = SettingsHelper.GetBoolean("IsDrawFaceRectangle");

        [ObservableProperty]
        private string _cameraImageSource = $"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/cameraEmpty.jpg";

        [ObservableProperty]
        private ObservableCollection<Models.FaceImage> _faceImageList = [];

        [ObservableProperty]
        private int _faceCount = 0;

        [ObservableProperty]
        private bool _isAddButtonEnabled = false;

        public AddFaceViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            DevicesName = [.. FaceRecognition.SystemCameraDevices.Keys];
            if (DevicesName[0] != "暂无摄像头")
            {
                IsOpenCameraButtonEnabled = true;
            }
        }

        partial void OnDeviceSelectedIndexChanged(int value)
        {
            IsOpenCameraButtonEnabled = true;
        }

        partial void OnIsDrawFaceRectangleChanged(bool value)
        {
            SettingsHelper.SetConfig("IsDrawFaceRectangle", value.ToString());
        }

        public void OnNavigatedTo()
        {

        }

        public void OnNavigatedFrom()
        {
            FaceRecognition.CloseCamera();
        }

        public void OpenCamera(System.Windows.Controls.Image image, System.Windows.Controls.Image maskImage)
        {
            if (!IsCameraOpened)
            {
                IsOpenCameraButtonEnabled = false;
                Thread video_thread = new(() => PlayVideo(image, maskImage))
                {
                    IsBackground = true
                };
                video_thread.Start();
            }
            else
            {
                CloseCamera(image, maskImage);
            }
        }

        private void CloseCamera(System.Windows.Controls.Image image, System.Windows.Controls.Image maskImage)
        {
            IsCameraOpened = false;
            FaceRecognition.CloseCamera();

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new()
            {
                Interval = TimeSpan.FromMicroseconds(500)
            };
            dispatcherTimer.Tick += (_, _) =>
            {
                image.Source = ImageProcess.StringToImageSource($"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/cameraEmpty.jpg");
                maskImage.Source = ImageProcess.StringToImageSource(string.Empty); dispatcherTimer.Stop();
            };
            dispatcherTimer.Start();
        }

        private void PlayVideo(System.Windows.Controls.Image cameraImage, System.Windows.Controls.Image maskImage)
        {
            if (FaceRecognition.SystemCameraDevices.TryGetValue(DevicesName[DeviceSelectedIndex], out int videoCapture_id))
            {
                if (FaceRecognition.OpenCamera(videoCapture_id, out string message))
                {
                    IsCameraOpened = true;
                    IsOpenCameraButtonEnabled = true;
                }
                else
                {
                    _snackbarService.Show("错误", $"开启摄像头失败：{message}", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
                    return;
                }
            }

            int sleepTime = FaceRecognition.SleepTime;

            while (FaceRecognition.IsCameraOpened)
            {
                using System.Drawing.Image image = FaceRecognition.GetImage();

                if (IsDrawFaceRectangle)
                {
                    maskImage.Dispatcher.Invoke(new Action(() => { maskImage.Source = ImageProcess.BitmapToPngBitmapImage((Bitmap)FaceRecognition.GetMaskImage(image)); }));
                }

                cameraImage.Dispatcher.Invoke(new Action(() => { cameraImage.Source = ImageProcess.BitmapToBitmapImage((Bitmap)image); }));

                Thread.Sleep(sleepTime);
            }
        }

        public async void CaptureFace(System.Windows.Controls.Image cameraImage)
        {
            using System.Drawing.Image faceImage = await FaceRecognition.GetFace(ImageProcess.ImageSourceToBitmap(cameraImage.Source));
            if (faceImage.Width == 1)
            {
                _snackbarService.Show("警告", $"未识别到人脸，无法添加！", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
            }
            else
            {
                FaceImageList.Add(new Models.FaceImage(ImageProcess.BitmapToBitmapImage((Bitmap)faceImage)));
                IsAddButtonEnabled = true;
                FaceCount++;
            }
        }

        public async void AddFace(System.Windows.Controls.Image image, System.Windows.Controls.Image maskImage)
        {
            if (FaceImageList.Count < 5)
            {
                if (await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "提示",
                    Content = "您的人脸图片数据太少，会导致训练识别结果精确度下降，是否继续添加？",
                    PrimaryButtonText = "是",
                    CloseButtonText = "否",
                }) != ContentDialogResult.Primary)
                {
                    return;
                }
            }
            
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
