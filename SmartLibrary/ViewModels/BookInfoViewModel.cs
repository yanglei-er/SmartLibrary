using CommunityToolkit.Mvvm.Messaging;
using SmartLibrary.Helpers;
using Wpf.Ui;

namespace SmartLibrary.ViewModels
{
    public partial class BookInfoViewModel : ObservableObject
    {
        private readonly SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
        private readonly LocalStorage localStorage = new();
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;

        [ObservableProperty]
        private bool _isPictureLoading = false;

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

        public BookInfoViewModel(INavigationService navigationService, ISnackbarService snackbarService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;

            localStorage.LoadingCompleted += LoadingCompleted;
            WeakReferenceMessenger.Default.Register<string, string>(this, "BookInfo", OnMessageReceived);
        }

        private async void OnMessageReceived(object recipient, string message)
        {
            IsbnText = message;
            Models.BookInfo bookInfo = await BooksDb.GetOneBookInfoAsync(IsbnText);
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
        }

        private void LoadingCompleted(string path)
        {
            Picture = path;
            IsPictureLoading = false;
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

        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.GoBack();
        }
    }
}