using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SmartLibrary.Helpers;
using SmartLibrary.Views.Pages;
using System.IO;
using System.Text;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartLibrary.ViewModels
{
    public partial class EditBookViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
        private readonly LocalStorage localStorage = new();

        [ObservableProperty]
        private bool _isPictureLoading = false;

        [ObservableProperty]
        private string _pictureLoadingText = "添加图片";

        [ObservableProperty]
        public bool _isEditButtonEnabled = false;

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

        private string PictureUrl = string.Empty;

        [ObservableProperty]
        private string _shelfNum = string.Empty;

        [ObservableProperty]
        private bool _isBorrowed = false;

        public EditBookViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
            WeakReferenceMessenger.Default.Register<string, string>(this, "EditBook", OnMessageReceived);
            localStorage.LoadingCompleted += LoadingCompleted;
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
            PictureUrl = bookInfo.Picture ?? string.Empty;

            IsPictureLoading = true;
            PictureLoadingText = "正在加载图片";
            localStorage.GetPicture(IsbnText, bookInfo.Picture);

            ShelfNum = bookInfo.ShelfNumber.ToString();
            IsBorrowed = bookInfo.IsBorrowed;
            WeakReferenceMessenger.Default.Unregister<string>(this);
        }

        private void LoadingCompleted(string path)
        {
            if (path == "Error")
            {
                PictureLoadingText = "图片加载失败";
                Picture = string.Empty;
            }
            else
            {
                Picture = path;
                PictureLoadingText = "添加图片";
            }
            IsPictureLoading = false;
        }

        [RelayCommand]
        private async Task OnSelectPictureButtonClickAsync()
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "选择图书封面图片",
                Filter = "图像文件|*.jpg;*.png;*.jpeg;*.bmp|所有文件|*.*",
                InitialDirectory = Environment.CurrentDirectory + @".\pictures\"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    Picture = openFileDialog.FileName;
                    PictureUrl = openFileDialog.FileName;
                    IsEditButtonEnabled = true;
                }
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
                if (await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "更改书籍信息",
                    Content = "是否将书籍图片设为空？",
                    PrimaryButtonText = "是",
                    CloseButtonText = "否",
                }) == ContentDialogResult.Primary)
                {
                    Picture = string.Empty;
                    PictureUrl = string.Empty;
                    IsEditButtonEnabled = true;
                }
            }
        }

        [RelayCommand]
        private void OnBorrowedButtonClick()
        {
            IsBorrowed = !IsBorrowed;
            IsEditButtonEnabled = true;
        }

        [RelayCommand]
        private async Task OnEditBookButtonClick()
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
                Models.BookInfo bookInfo = new()
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
                    Language = Language,
                    Picture = LocalStorage.AddPicture(IsbnText, PictureUrl),
                    ShelfNumber = int.Parse(ShelfNum),
                    IsBorrowed = IsBorrowed,
                };
                BooksDb.UpdateAsync(bookInfo);
                System.Media.SystemSounds.Asterisk.Play();
                _snackbarService.Show("更改成功", $"书籍《{BookName}》信息已更新。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                IsEditButtonEnabled = false;
                WeakReferenceMessenger.Default.Send(string.Empty, "BookManage");
            }
            else
            {
                string content = "您必须完善以下书籍信息， 才能更改书籍信息：\n\n" + tip.ToString();
                System.Media.SystemSounds.Asterisk.Play();
                await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "更改书籍信息",
                    Content = content,
                    CloseButtonText = "去完善",
                });
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
        private void NavigateBack()
        {
            localStorage.LoadingCompleted -= LoadingCompleted;
            _navigationService.Navigate(typeof(BookManage));
        }
    }
}