using CommunityToolkit.Mvvm.Messaging;
using SmartLibrary.Helpers;
using SmartLibrary.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartLibrary.ViewModels
{
    public partial class Borrow_Return_BookViewModel : ObservableObject
    {
        private readonly SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
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
        private string _isbnAttitudeImage = "pack://application:,,,/Assets/pic/wrong.png";

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
        private string _words = string.Empty;

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

        public Borrow_Return_BookViewModel(INavigationService navigationService, ISnackbarService snackbarService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;

            localStorage.LoadingCompleted += LoadingCompleted;
            BluetoothHelper.ReceiveEvent += OnBluetoothReceived;
            WeakReferenceMessenger.Default.Register<string, string>(this, "Borrow_Return_Book", OnMessageReceived);

            if (BluetoothHelper.IsBleConnected)
            {
                IsScanButtonVisible = true;
            }
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
                BookInfo bookInfo = await BooksDb.GetOneBookInfoAsync(IsbnText);
                BookName = bookInfo.BookName;
                Author = bookInfo.Author;
                Press = bookInfo.Press ?? string.Empty;
                PressDate = bookInfo.PressDate ?? string.Empty;
                PressPlace = bookInfo.PressPlace ?? string.Empty;
                Price = bookInfo.Price ?? string.Empty;
                ClcName = bookInfo.ClcName ?? string.Empty;
                Words = bookInfo.Words ?? string.Empty;
                Pages = bookInfo.Pages ?? string.Empty;
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
                    IsbnAttitudeImage = "pack://application:,,,/Assets/pic/right.png";
                    IsSearchButtonEnabled = true;
                }
                else
                {
                    IsbnAttitudeImage = "pack://application:,,,/Assets/pic/wrong.png";
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
        private void BorrowOrReturn()
        {
            if (IsBorrowed)
            {
                if (BluetoothHelper.IsBleConnected)
                {
                    _snackbarService.Show("操作成功", $"{BookNameText}已还，小车即将启动，把书送回{ShelfNum}号书架。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                    BluetoothHelper.Send("");
                }
                else
                {
                    _snackbarService.Show("操作成功", $"{BookNameText}已还，蓝牙未连接，小车无法自动将书送回", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
                BooksDb.ReturnBookAsync(IsbnText);
            }
            else
            {
                _snackbarService.Show("操作成功", $"{BookNameText}已借出", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                BooksDb.BorrowBookAsync(IsbnText);
            }
            IsBorrowed = !IsBorrowed;
            WeakReferenceMessenger.Default.Send("refresh", "BookInfo");
            WeakReferenceMessenger.Default.Send("refresh", "BookManage");
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
            Words = string.Empty;
            Pages = string.Empty;
            BookDesc = string.Empty;
            Language = string.Empty;
            Picture = "pack://application:,,,/Assets/PictureEmpty.png";
            ShelfNum = string.Empty;
            IsBorrowed = false;
            Borrow_Return_ButtonEnabled = false;
        }
    }
}