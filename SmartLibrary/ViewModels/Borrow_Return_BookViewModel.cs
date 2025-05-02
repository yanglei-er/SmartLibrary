using CommunityToolkit.Mvvm.Messaging;
using Shared.Helpers;
using SmartLibrary.Helpers;
using SmartLibrary.Models;
using System.Drawing;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace SmartLibrary.ViewModels
{
    public partial class Borrow_Return_BookViewModel : ObservableObject, INavigationAware
    {
        private readonly BooksDb BooksDb = BooksDb.GetDatabase("books.smartlibrary");
        private readonly LocalStorage localStorage = new();
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private bool needRefresh = false;

        private readonly UsersDb UsersDb = UsersDb.GetDatabase("users.smartmanager");
        private int TotalCount = 0;
        private int _deviceIndex = SettingsHelper.GetInt("DeviceIndex");

        [ObservableProperty]
        private bool _isNotCamerasAvailable = false;

        [ObservableProperty]
        private bool _isSearchButtonEnabled = false;

        [ObservableProperty]
        private bool _isbnAttitudeVisible = false;

        [ObservableProperty]
        private string _isbnAttitudeImage = "pack://application:,,,/Assets/wrong.png";

        [ObservableProperty]
        private bool _isPictureLoading = false;

        [ObservableProperty]
        private bool _borrow_Return_ButtonEnabled = false;

        [ObservableProperty]
        private string _isbnText = string.Empty;

        [ObservableProperty]
        private string _bookNameText = string.Empty;

        [ObservableProperty]
        private string _authorText = string.Empty;

        [ObservableProperty]
        private string _pressText = string.Empty;

        [ObservableProperty]
        private string _bookName = string.Empty;

        [ObservableProperty]
        private string _author = string.Empty;

        [ObservableProperty]
        private string _press = string.Empty;

        [ObservableProperty]
        private string _pressDate = string.Empty;

        [ObservableProperty]
        private string _pressPlace = string.Empty;

        [ObservableProperty]
        private string _price = string.Empty;

        [ObservableProperty]
        private string _pages = string.Empty;

        [ObservableProperty]
        private string _keyword = string.Empty;

        [ObservableProperty]
        private string _clcName = string.Empty;

        [ObservableProperty]
        private string _bookDesc = string.Empty;

        [ObservableProperty]
        private string _language = string.Empty;

        [ObservableProperty]
        private string _picture = string.Empty;

        [ObservableProperty]
        private string _shelfNum = string.Empty;

        [ObservableProperty]
        private bool _isBorrowed = false;

        [ObservableProperty]
        private List<string> _devicesName = [.. FaceRecognition.SystemCameraDevices.Keys];

        [ObservableProperty]
        private bool _isCameraOpened = false;

        public Borrow_Return_BookViewModel(INavigationService navigationService, ISnackbarService snackbarService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;

            localStorage.LoadingCompleted += LoadingCompleted;
            WeakReferenceMessenger.Default.Register<string, string>(this, "Borrow_Return_Book", OnMessageReceived);

            if (UsersDb.IsDatabaseConnected("users.smartmanager"))
            {
                TotalCount = UsersDb.GetRecordCount();
            }


            if (DevicesName[0] == "暂无摄像头")
            {
                IsNotCamerasAvailable = true;
            }

            if (_deviceIndex > DevicesName.Count)
            {
                _deviceIndex = 0;
                SettingsHelper.SetConfig("DeviceIndex", "0");
            }
        }

        public Task OnNavigatedToAsync()
        {
            if (needRefresh)
            {
                needRefresh = false;
                _ = OnSearchButtonClick();
            }
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        private void OnMessageReceived(object recipient, string message)
        {
            if (message.StartsWith('.'))
            {
                if (IsbnText == message.Remove(0, 1))
                {
                    needRefresh = true;
                }
            }
            else if (message == "refresh")
            {
                needRefresh = true;
            }
            else if (message == "deviceRefresh")
            {
                DevicesName = [.. FaceRecognition.SystemCameraDevices.Keys];
                _deviceIndex = SettingsHelper.GetInt("DeviceIndex");
            }
            else if (message == "refreshUser")
            {
                TotalCount = UsersDb.GetRecordCount();
            }
            else
            {
                IsbnText = message;
                _ = OnSearchButtonClick();
            }
        }

        private void LoadingCompleted(string path)
        {
            Picture = path;
            IsPictureLoading = false;
        }

        [RelayCommand]
        public async Task OnSearchButtonClick()
        {
            if (await BooksDb.ExistsAsync(IsbnText))
            {
                BookInfo bookInfo = await BooksDb.GetOneBookInfoAsync(IsbnText);
                BookName = bookInfo.BookName;
                Author = bookInfo.Author;
                Press = bookInfo.Press ?? string.Empty;
                PressDate = bookInfo.PressDate ?? string.Empty;
                PressPlace = bookInfo.PressPlace ?? string.Empty;
                Price = bookInfo.Price ?? string.Empty;
                ClcName = bookInfo.ClcName ?? string.Empty;
                Pages = bookInfo.Pages ?? string.Empty;
                Keyword = bookInfo.Keyword ?? string.Empty;
                BookDesc = bookInfo.BookDesc ?? string.Empty;
                Language = bookInfo.Language ?? string.Empty;

                IsPictureLoading = true;
                localStorage.GetPicture(IsbnText, bookInfo.Picture);

                ShelfNum = bookInfo.ShelfNumber.ToString();
                IsBorrowed = bookInfo.IsBorrowed;

                Borrow_Return_ButtonEnabled = true;
            }
            else
            {
                CleanExceptIsbn();
                _snackbarService.Show("查无此书", "此书还未收录到数据库中！", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
            }
        }

        partial void OnIsbnTextChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                IsbnAttitudeVisible = false;
                IsSearchButtonEnabled = false;
                CleanExceptIsbn();
            }
            else
            {
                IsbnAttitudeVisible = true;
                if (value.Length == 13)
                {
                    IsbnAttitudeImage = "pack://application:,,,/Assets/right.png";
                    IsSearchButtonEnabled = true;
                }
                else
                {
                    IsbnAttitudeImage = "pack://application:,,,/Assets/wrong.png";
                    IsSearchButtonEnabled = false;
                    CleanExceptIsbn();
                }
            }
        }

        partial void OnBookNameChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                BookNameText = string.Empty;
            }
            else
            {
                BookNameText = $"《{value}》";
            }
        }

        partial void OnAuthorChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                AuthorText = string.Empty;
            }
            else
            {
                AuthorText = "作者：" + value;
            }
        }

        partial void OnPressChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                PressText = string.Empty;
            }
            else
            {
                PressText = "出版社：" + value;
            }
        }

        private void PlayVideo(System.Windows.Controls.Image cameraImage)
        {
            if (FaceRecognition.SystemCameraDevices.TryGetValue(DevicesName[0], out int videoCapture_id))
            {
                FaceRecognition.OpenCamera(videoCapture_id, out string message);
            }

            int sleepTime = FaceRecognition.SleepTime;
            string name = string.Empty;
            int scanTime = 0;
            int timeOut = SettingsHelper.GetInt("TimedOut") * 12;

            while (FaceRecognition.IsCameraOpened && scanTime < timeOut)
            {
                using Bitmap image = FaceRecognition.GetImage();

                cameraImage.Dispatcher.Invoke(new Action(() => { cameraImage.Source = ImageProcess.BitmapToBitmapImage(image); }));

                (Bitmap, float[], int, int) result = FaceRecognition.GetMaskAndName(image);
                float[] feature = result.Item2;

                if (feature.Length != 0)
                {
                    for (int i = 0; i < TotalCount; i++)
                    {
                        if (FaceRecognition.IsSelf(feature, FaceRecognition.GetFaceFeatureFromString(UsersDb.GetOneFaceFeatureStringByIndex(i))))
                        {
                            name = UsersDb.GetOneNameByIndex(i);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        break;
                    }
                }
                scanTime++;
                Thread.Sleep(sleepTime);
            }
            cameraImage.Dispatcher.BeginInvoke(new Action(() =>
            {
                Success(name);
            }));
        }

        [RelayCommand]
        private void StopCamera()
        {
            FaceRecognition.CloseCamera();
            IsCameraOpened = false;
        }

        public void BorrowOrReturn(System.Windows.Controls.Image image)
        {
            IsCameraOpened = true;
            Thread video_thread = new(() => PlayVideo(image))
            {
                IsBackground = true
            };
            video_thread.Start();
        }

        private void Success(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (IsBorrowed)
                {
                    _snackbarService.Show("操作成功", $"用户 {name} 已将{BookNameText}还回", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                    BooksDb.ReturnBookAsync(IsbnText);
                }
                else
                {
                    _snackbarService.Show("操作成功", $"用户 {name} 已将{BookNameText}借出", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                    BooksDb.BorrowBookAsync(IsbnText);
                }
                IsBorrowed = !IsBorrowed;
                WeakReferenceMessenger.Default.Send("refresh", "BookManage");
            }
            else if (IsCameraOpened)
            {
                _snackbarService.Show("操作失败", "该用户未录入数据库，请先录入人脸数据。", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
            }
            FaceRecognition.CloseCamera();
            IsCameraOpened = false;
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.Navigate(typeof(Views.Pages.Bookshelf));
        }

        private void CleanExceptIsbn()
        {
            BookName = string.Empty;
            Author = string.Empty;
            Press = string.Empty;
            PressDate = string.Empty;
            PressPlace = string.Empty;
            Price = string.Empty;
            ClcName = string.Empty;
            Keyword = string.Empty;
            Pages = string.Empty;
            BookDesc = string.Empty;
            Language = string.Empty;
            Picture = ResourceManager.EmptyImage;
            ShelfNum = string.Empty;
            IsBorrowed = false;
            Borrow_Return_ButtonEnabled = false;
        }
    }
}