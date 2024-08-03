using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Shared.Helpers;
using Shared.Models;
using SmartManager.Helpers;
using System.Collections.ObjectModel;
using System.Drawing;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using Yitter.IdGenerator;

namespace SmartManager.ViewModels
{
    public partial class AddFaceViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly Database FacesDb = Database.GetDatabase("faces.smartmanager");
        private bool Unknown = true;
        private int TotalCount = 0;
        private bool IsAddingFace = false;

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
        private bool _isFaceComparison = SettingsHelper.GetBoolean("IsFaceComparison");

        [ObservableProperty]
        private string _cameraImageSource = $"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/cameraEmpty.jpg";

        [ObservableProperty]
        private ObservableCollection<EncodingFace> _faceList = [];

        [ObservableProperty]
        private int _faceCount = 0;

        [ObservableProperty]
        private bool _isAddButtonEnabled = false;

        [ObservableProperty]
        private string _uID = YitIdHelper.NextId().ToString();

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _sex = string.Empty;

        [ObservableProperty]
        private string _age = string.Empty;

        [ObservableProperty]
        private string _joinTime = DateTime.Now.ToString("d");

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

            TotalCount = FacesDb.GetRecordCount();
        }

        partial void OnDeviceSelectedIndexChanged(int value)
        {
            IsOpenCameraButtonEnabled = true;
        }

        partial void OnIsDrawFaceRectangleChanged(bool value)
        {
            SettingsHelper.SetConfig("IsDrawFaceRectangle", value.ToString());
        }

        partial void OnIsFaceComparisonChanged(bool value)
        {
            SettingsHelper.SetConfig("IsFaceComparison", value.ToString());
        }

        public void DeviceComboBox_DropDownOpened()
        {
            DevicesName = [.. FaceRecognition.SystemCameraDevices.Keys];
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
                Interval = TimeSpan.FromMicroseconds(200)
            };
            dispatcherTimer.Tick += (_, _) =>
            {
                image.Source = ImageProcess.StringToBitmapImage($"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/cameraEmpty.jpg");
                maskImage.Source = ImageProcess.StringToBitmapImage(string.Empty); dispatcherTimer.Stop();
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
                    _snackbarService.Show("错误", $"开启摄像头失败：{message}", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                    return;
                }
            }

            int sleepTime = FaceRecognition.SleepTime;

            while (FaceRecognition.IsCameraOpened)
            {
                if (IsAddingFace)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    using Bitmap image = FaceRecognition.GetImage();

                    if (IsFaceComparison)
                    {
                        (Bitmap, float[], int, int) result = FaceRecognition.GetMaskAndName(image);
                        Bitmap mask = result.Item1;
                        float[] feature = result.Item2;

                        if (feature.Length != 0)
                        {
                            Unknown = true;

                            for (int i = 0; i < TotalCount; i++)
                            {
                                if (FaceRecognition.IsSelf(feature, FaceRecognition.GetFaceFeatureFromString(FacesDb.GetOneFaceFeatureStringByIndex(i))))
                                {
                                    mask.DrawText(FacesDb.GetOneNameByIndex(i), result.Item3, result.Item4);
                                    Unknown = false;
                                    break;
                                }
                            }
                            if (Unknown)
                            {
                                mask.DrawText("未知", result.Item3, result.Item4);
                            }
                        }
                        maskImage.Dispatcher.Invoke(new Action(() => { maskImage.Source = ImageProcess.BitmapToPngBitmapImage(mask); }));
                    }
                    else if (IsDrawFaceRectangle)
                    {
                        maskImage.Dispatcher.Invoke(new Action(() => { maskImage.Source = ImageProcess.BitmapToPngBitmapImage(FaceRecognition.GetMaskImage(image)); }));
                    }

                    cameraImage.Dispatcher.Invoke(new Action(() => { cameraImage.Source = ImageProcess.BitmapToBitmapImage(image); }));

                    Thread.Sleep(sleepTime);
                }
            }
        }

        public async void CaptureFace(System.Windows.Controls.Image cameraImage)
        {
            EncodingFace face = await FaceRecognition.GetFace(ImageProcess.ImageSourceToBitmap(cameraImage.Source));

            if (face.FullImage.Width == 2)
            {
                _snackbarService.Show("警告", "未识别到人脸，无法添加！", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
            }
            else
            {
                FaceList.Add(face);
                IsAddButtonEnabled = true;
                FaceCount++;
            }
        }

        [RelayCommand]
        private async Task ImportImage()
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "导入图像",
                Filter = "图像文件 (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png",
                Multiselect = true,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                List<string> fileNames = new(openFileDialog.FileNames);
                int MissingFace = 0;
                foreach (string fileName in fileNames)
                {
                    EncodingFace face = await FaceRecognition.GetFace((Bitmap)Bitmap.FromFile(fileName));
                    if (face.FullImage.Width == 2)
                    {
                        MissingFace++;
                    }
                    else
                    {
                        FaceList.Add(face);
                        IsAddButtonEnabled = true;
                        FaceCount++;
                    }
                }
                if (MissingFace > 0)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    _snackbarService.Show("警告", $"{MissingFace}张图片未识别到人脸，无法添加！", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
                }
            }
        }

        [RelayCommand]
        private void RefreshUID()
        {
            UID = YitIdHelper.NextId().ToString();
        }

        public async void AddFace()
        {
            if (await FacesDb.ExistsAsync(UID))
            {
                _snackbarService.Show("添加失败", $"用户 {UID} 已存在，无法添加。", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                return;
            }

            IsAddingFace = true;

            if (!Unknown)
            {
                System.Media.SystemSounds.Asterisk.Play();
                if (await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "录入人脸",
                    Content = "已检测到数据库中存在相似人脸，是否继续添加？",
                    PrimaryButtonText = "是",
                    CloseButtonText = "否",
                }) == ContentDialogResult.Secondary)
                {
                    IsAddingFace = false;
                    return;
                }
            }

            if (string.IsNullOrEmpty(Name))
            {
                System.Media.SystemSounds.Asterisk.Play();
                await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "录入人脸",
                    Content = "您必须完善以下信息， 才能将人脸数据添加到数据库中：\n\n姓名不能为空！",
                    CloseButtonText = "去完善",
                });
                IsAddingFace = false;
                return;
            }

            if (FaceList.Count < 3)
            {
                if (await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "录入人脸",
                    Content = "您的人脸图片数据太少(<3)，可能会导致训练识别结果精确度下降，是否继续添加？",
                    PrimaryButtonText = "是",
                    CloseButtonText = "否",
                }) != ContentDialogResult.Primary)
                {
                    IsAddingFace = false;
                    return;
                }
            }

            if (string.IsNullOrEmpty(UID))
            {
                UID = YitIdHelper.NextId().ToString();
            }

            if (string.IsNullOrEmpty(JoinTime))
            {
                JoinTime = DateTime.Now.ToString("d");
            }

            List<float> faceQualitys = FaceRecognition.GetFaceQualitys([.. FaceList]);
            int MaxIndex = faceQualitys.IndexOf(faceQualitys.Max());
            string faceFuture = FaceRecognition.GetFaceFeatureString(FaceList[MaxIndex]);

            FacesDb.AddFaceAsync(new(UID, Name, Sex, Age, JoinTime, faceFuture, FaceList[MaxIndex].FaceImage));
            TotalCount++;

            System.Media.SystemSounds.Asterisk.Play();
            _snackbarService.Show("添加成功", $"用户 {Name} 已添加到数据库中。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));

            WeakReferenceMessenger.Default.Send("refresh", "FaceManage");
            CleanAll();

            IsAddingFace = false;
        }

        private void CleanAll()
        {
            UID = YitIdHelper.NextId().ToString();
            Name = string.Empty;
            Sex = string.Empty;
            Age = string.Empty;
            JoinTime = DateTime.Now.ToString("d");
            FaceList.Clear();
            FaceCount = 0;
            IsAddButtonEnabled = false;
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
