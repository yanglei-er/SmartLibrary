using Microsoft.Win32;
using SmartLibrary.Helpers;
using SmartLibrary.Models;
using System.IO;
using System.Text;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartLibrary.ViewModels
{
    public partial class AddBookViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");

        [ObservableProperty]
        private string _isbnBoxPlaceholderText = "请输入13位ISBN码";
        [ObservableProperty]
        private bool _isbnAttitudeVisible = false;
        [ObservableProperty]
        private string _isbnAttitudeImage = string.Empty;
        [ObservableProperty]
        private bool _isScanButtonEnabled = false;
        [ObservableProperty]
        private bool _isSearchButtonEnabled = false;

        [ObservableProperty]
        private bool _isBookExisted = false;
        [ObservableProperty]
        private bool _isAddButtonEnabled = false;

        [ObservableProperty]
        private string _bookNameText = string.Empty;
        [ObservableProperty]
        private string _authorText = string.Empty;
        [ObservableProperty]
        private string _pressText = string.Empty;

        [ObservableProperty]
        private string _isbnText = string.Empty;
        [ObservableProperty]
        private string _picture = string.Empty;
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
        private string _shelfNum = string.Empty;
        [ObservableProperty]
        private bool _isBorrowed = false;

        public AddBookViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            if (BluetoothHelper.Instance.IsBleConnected)
            {
                IsbnBoxPlaceholderText = "请扫描或输入13位ISBN码";
                IsScanButtonEnabled = true;
            }
        }

        [RelayCommand]
        private void OnScanButtonClick()
        {

        }

        [RelayCommand]
        public void OnSearchButtonClick(string parameter)
        {
            if (BooksDb.Exists(parameter))
            {
                System.Media.SystemSounds.Asterisk.Play();

                IsBookExisted = true;
                IsSearchButtonEnabled = false;
                IsAddButtonEnabled = false;
                CleanExceptIsbn();

                BookInfo bookInfo = BooksDb.GetOneBookInfo(parameter);
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
                ShelfNum = bookInfo.ShelfNumber.ToString();
                IsBorrowed = bookInfo.IsBorrowed;
                Picture = bookInfo.Picture ?? string.Empty;
            }
            else
            {
                IsSearchButtonEnabled = false;
                IsAddButtonEnabled = true;
                CleanExceptIsbn();
            }
        }

        partial void OnIsbnTextChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                IsbnAttitudeVisible = false;
                IsSearchButtonEnabled = false;
                IsAddButtonEnabled = false;
                CleanExceptIsbn();
                IsBookExisted = false;
                return;
            }
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
                IsAddButtonEnabled = false;
                CleanExceptIsbn();
                IsBookExisted = false;
            }
        }

        [RelayCommand]
        private void OnSelectPictureButtonClick()
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "选择图书封面图片",
                Filter = "图像文件|*.jpg;*.png;*.jpeg;*.bmp|所有文件|*.*",
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    Picture = openFileDialog.FileName;
                }
            }
        }

        [RelayCommand]
        private void OnBorrowedButtonClick()
        {
            IsBorrowed = !IsBorrowed;
        }

        [RelayCommand]
        private async Task OnAddBookButtonClick()
        {
            StringBuilder tip = new();
            if (string.IsNullOrEmpty(BookName))
            {
                tip.AppendLine("书名不能为空！");
            }
            if (string.IsNullOrEmpty(Author))
            {
                tip.AppendLine("作者不能为空！");
            }
            if (string.IsNullOrEmpty(ShelfNum))
            {
                tip.AppendLine("书架号不能为空！");
            }

            if (string.IsNullOrEmpty(tip.ToString()))
            {
                BookInfo bookInfo = new()
                {
                    Isbn = IsbnText,
                    BookName = BookName,
                    Author = Author,
                    Press = Press,
                    PressDate = PressDate,
                    PressPlace = PressPlace,
                    Price = Price,
                    ClcName = ClcName,
                    Words = Words,
                    Pages = Pages,
                    BookDesc = BookDesc,
                    ShelfNumber = int.Parse(ShelfNum),
                    IsBorrowed = IsBorrowed,
                    Picture = Picture
                };
                BooksDb.AddBook(bookInfo);
                System.Media.SystemSounds.Asterisk.Play();
                _snackbarService.Show("添加成功", $"书籍《{BookName}》已添加到数据库中。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                IsbnText = string.Empty;
            }
            else
            {
                string content = "您必须完善以下书籍信息， 才能将书籍添加到数据库中：\n\n" + tip.ToString();
                System.Media.SystemSounds.Asterisk.Play();
                await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "添加书籍",
                    Content = content,
                    CloseButtonText = "好的",
                });
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
            Words = string.Empty;
            Pages = string.Empty;
            BookDesc = string.Empty;
            ShelfNum = string.Empty;
            IsBorrowed = false;
            Picture = string.Empty;
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
    }
}
