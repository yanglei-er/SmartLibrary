using CommunityToolkit.Mvvm.Messaging;
using SmartLibrary.Helpers;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartLibrary.ViewModels
{
    public partial class BookInfoViewModel : ObservableObject, INavigationAware
    {
        private readonly BooksDb BooksDb = BooksDb.GetDatabase("books.smartlibrary");
        private readonly LocalStorage localStorage = new();
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;

        [ObservableProperty]
        private bool _isScanButtonEnabled = true;

        [ObservableProperty]
        private bool _isScanButtonVisible = false;

        [ObservableProperty]
        private bool _isSearchButtonEnabled = false;

        [ObservableProperty]
        private bool _isbnAttitudeVisible = false;

        [ObservableProperty]
        private string _isbnAttitudeImage = "pack://application:,,,/Assets/wrong.png";

        [ObservableProperty]
        private bool _isPictureLoading = false;

        [ObservableProperty]
        private bool _goButtonEnabled = false;

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

        public BookInfoViewModel(INavigationService navigationService, ISnackbarService snackbarService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;

            BluetoothHelper.ReceiveEvent += OnBluetoothReceived;
            localStorage.LoadingCompleted += LoadingCompleted;
            WeakReferenceMessenger.Default.Register<string, string>(this, "BookInfo", OnMessageReceived);
        }

        public void OnNavigatedTo()
        {
            if (BluetoothHelper.IsBleConnected)
            {
                IsScanButtonVisible = true;
            }
            else
            {
                IsScanButtonVisible = false;
            }
        }

        public void OnNavigatedFrom()
        {

        }

        private async void OnMessageReceived(object recipient, string message)
        {
            if (message.StartsWith('.'))
            {
                if (IsbnText != message.Remove(0, 1))
                {
                    return;
                }
            }
            else if (message == "refresh")
            {

            }
            else
            {
                IsbnText = message;
            }
            if (await BooksDb.ExistsAsync(IsbnText))
            {
                Models.BookInfo bookInfo = await BooksDb.GetOneBookInfoAsync(IsbnText);
                BookName = bookInfo.BookName;
                Author = bookInfo.Author;
                Press = bookInfo.Press ?? string.Empty;
                PressDate = bookInfo.PressDate ?? string.Empty;
                PressPlace = bookInfo.PressPlace ?? string.Empty;
                Price = bookInfo.Price ?? string.Empty;
                ClcName = bookInfo.ClcName ?? string.Empty;
                Keyword = bookInfo.Keyword ?? string.Empty;
                Pages = bookInfo.Pages ?? string.Empty;
                BookDesc = bookInfo.BookDesc ?? string.Empty;
                Language = bookInfo.Language ?? string.Empty;

                IsPictureLoading = true;
                localStorage.GetPicture(IsbnText, bookInfo.Picture);

                ShelfNum = bookInfo.ShelfNumber.ToString();
                IsBorrowed = bookInfo.IsBorrowed;

                GoButtonEnabled = true;
            }
            else
            {
                CleanExceptIsbn();
            }
        }

        private void LoadingCompleted(string path)
        {
            Picture = path;
            IsPictureLoading = false;
        }

        [RelayCommand]
        private void OnScanButtonClick()
        {
            _snackbarService.Show("正在扫描", $"请将书置于亚克力板上", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
            BluetoothHelper.Send("scan");
            IsScanButtonEnabled = false;
        }

        private void OnBluetoothReceived(string info)
        {
            IsScanButtonEnabled = true;
            if (info.StartsWith("978") && info.Length == 13)
            {
                IsbnText = info;
                OnMessageReceived(this, IsbnText);
            }
            else if (info == "over")
            {
                _snackbarService.Show("操作成功", $"{BookNameText}已还至{ShelfNum}号书架", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(2));
            }
            else
            {
                _snackbarService.Show("条码错误", $"请重新扫描", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        [RelayCommand]
        public void OnSearchButtonClick()
        {
            OnMessageReceived(this, IsbnText);
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

        [RelayCommand]
        private void Go()
        {
            _navigationService.Navigate(typeof(Views.Pages.Borrow_Return_Book));

            if (BluetoothHelper.IsBleConnected)
            {
                _snackbarService.Show("小车已启动", $"请前往{ShelfNum}号书架", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                BluetoothHelper.SendOnly($"带我去,{ShelfNum}");
            }
            else
            {
                _snackbarService.Show("蓝牙未连接", $"小车无法启动，请自行前往{ShelfNum}号书架", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.GoBack();
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
            GoButtonEnabled = false;
        }
    }
}