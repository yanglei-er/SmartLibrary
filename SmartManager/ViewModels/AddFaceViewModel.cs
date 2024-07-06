using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Shared.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SmartManager.Helpers;
using SmartManager.Models;
using System.Collections.ObjectModel;
using System.IO;
using ViewFaceCore;
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
        private Wpf.Ui.Controls.Image cameraImage = new();

        [ObservableProperty]
        private List<string> _devicesName = [];

        [ObservableProperty]
        private int _deviceSelectedIndex = 0;

        [ObservableProperty]
        private bool _isOpenCameraButtonEnabled = false;

        [ObservableProperty]
        private bool _isCameraOpened = false;

        [ObservableProperty]
        private bool _isDrawFaceFrame = false;

        [ObservableProperty]
        private string _cameraImageSource = $"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/cameraEmpty.jpg";

        [ObservableProperty]
        private ObservableCollection<FaceImage> _faceImageList = [];

        [ObservableProperty]
        private bool _isAddButtonEnabled = false;

        public AddFaceViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            DevicesName = [.. devices.Keys];
            if (DevicesName[0] != "暂无摄像头")
            {
                IsOpenCameraButtonEnabled = true;
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

        public void OpenCamera(Wpf.Ui.Controls.Image image)
        {
            if (!IsCameraOpened)
            {
                IsOpenCameraButtonEnabled = false;
                cameraImage = image;
                video_thread.Start();
            }
            else
            {
                IsCameraOpened = false;
                CameraImageSource = $"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/cameraEmpty.jpg";
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
                videoCapture.Set(VideoCaptureProperties.Fps, 2);

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

            using ViewFaceCore.Core.FaceDetector _faceDetector = new();

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

                if (IsDrawFaceFrame)
                {
                    using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(v_mat.ToMemoryStream());
                    var faceInfos = _faceDetector.Detect(image);

                    foreach (var face in faceInfos)
                    {
                        image.Mutate(x =>
                        {
                            x.Draw(Color.HotPink, 2.5f, new RectangleF(face.Location.X, face.Location.Y, face.Location.Width, face.Location.Height));
                        }
                        );
                    }

                    using MemoryStream outputStream = new();
                    image.Save(outputStream, new JpegEncoder() { Quality = 80 });
                    cameraImage.Dispatcher.Invoke(new Action(() => { cameraImage.Source = Utils.ByteToImageSource(outputStream.ToArray()); }));
                }
                else
                {
                    cameraImage.Dispatcher.Invoke(new Action(() => { cameraImage.Source = v_mat.ToBitmapSource(); }));
                }
                v_mat.Release();
            }
        }

        [RelayCommand]
        void Test()
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "选择图像",
                Filter = "jpg图像 (*.jpg)|*.jpg",
                Multiselect = false,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ViewFaceCore.Core.FaceDetector _faceDetector = new();
                using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(openFileDialog.FileName);
                var faceInfos = _faceDetector.Detect(image);
                foreach (var face in faceInfos)
                {
                    image.Mutate(x =>
                    {
                        x.Draw(Color.HotPink, 2.5f, new RectangleF(face.Location.X, face.Location.Y, face.Location.Width, face.Location.Height));
                    }
                    );
                }

                using MemoryStream outputStream = new();
                image.Save(outputStream, new JpegEncoder() { Quality = 80 });
                cameraImage.Source = Utils.ByteToImageSource(outputStream.ToArray()); ;
            }
        }

        [RelayCommand]
        void CollectingFace()
        {

        }

        [RelayCommand]
        void AddFace()
        {

        }

        [RelayCommand]
        void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
