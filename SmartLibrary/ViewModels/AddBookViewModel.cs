using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SmartLibrary.Helpers;
using SmartLibrary.Views.Pages;
using System.IO;
using System.Text;
using System.Text.Json;
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
        private bool _isbnBoxEnabled = true;

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
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isNetwrokError = false;

        [ObservableProperty]
        private string _networkErrorText = string.Empty;

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
        public async Task OnSearchButtonClick()
        {
            if (await BooksDb.ExistsAsync(IsbnText))
            {
                System.Media.SystemSounds.Asterisk.Play();

                IsBookExisted = true;
                IsSearchButtonEnabled = false;

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
                Picture = LocalStorage.GetPicture(IsbnText, bookInfo.Picture);
                ShelfNum = bookInfo.ShelfNumber.ToString();
                IsBorrowed = bookInfo.IsBorrowed;
            }
            else
            {
                IsSearchButtonEnabled = false;
                if (Network.IsInternetConnected())
                {
                    IsLoading = true;
                    IsbnBoxEnabled = false;
                    IsNetwrokError = false;
                    string result = await Network.GetAsync($"http://openapi.daohe168.com.cn/api/library/isbn/query?isbn={IsbnText}&appKey=d7c6c07a0a04ba4e65921e2f90726384");

                    if (result.StartsWith("Error"))
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        NetworkErrorText = "连接服务器失败，错误代码：" + result.Split(":")[1];
                        IsNetwrokError = true;
                    }
                    else
                    {
                        using JsonDocument jsondocument = JsonDocument.Parse(result);
                        JsonElement rootElement = jsondocument.RootElement;
                        if (rootElement.GetProperty("success").GetBoolean())
                        {
                            JsonElement dataElement = rootElement.GetProperty("data");
                            BookName = dataElement.GetProperty("bookName").GetString() ?? string.Empty;
                            Author = dataElement.GetProperty("author").GetString() ?? string.Empty;
                            Press = dataElement.GetProperty("press").GetString() ?? string.Empty;
                            PressDate = dataElement.GetProperty("pressDate").GetString() ?? string.Empty;
                            PressPlace = dataElement.GetProperty("pressPlace").GetString() ?? string.Empty;
                            Price = (dataElement.GetProperty("price").GetDouble() / 100).ToString();
                            ClcName = dataElement.GetProperty("clcName").GetString() ?? string.Empty;
                            Words = dataElement.GetProperty("words").GetString() ?? string.Empty;
                            Pages = dataElement.GetProperty("pages").GetString() ?? string.Empty;
                            BookDesc = dataElement.GetProperty("bookDesc").GetString() ?? string.Empty;
                            Language = dataElement.GetProperty("language").GetString() ?? string.Empty;
                            string? picture = dataElement.GetProperty("pictures").GetString();
                            if (!string.IsNullOrEmpty(picture))
                            {
                                Picture = LocalStorage.SearchPicture(IsbnText, picture.Replace("[\"", "").Replace("\"]", ""));
                            }
                        }
                        else
                        {
                            System.Media.SystemSounds.Asterisk.Play();
                            NetworkErrorText = rootElement.GetProperty("message").GetString() + ". 您可以手动录入书籍信息。" ?? "查询失败，原因未知。您可以手动录入书籍信息。";
                            IsNetwrokError = true;
                        }
                    }
                    IsLoading = false;
                }
                else
                {
                    IsSearchButtonEnabled = true;
                    System.Media.SystemSounds.Asterisk.Play();
                    NetworkErrorText = "网络未连接！无法查询联网数据库，请手动录入书籍信息或连接网络后重试。";
                    IsNetwrokError = true;
                }
            }
            IsbnBoxEnabled = true;
            IsAddButtonEnabled = true;
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
                IsNetwrokError = false;
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
                    IsAddButtonEnabled = false;
                    CleanExceptIsbn();
                    IsBookExisted = false;
                    IsNetwrokError = false;
                }
            }
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
                }
            }
            else
            {
                if (await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "更改书籍信息",
                    Content = "是否将书籍图片设为空？",
                    PrimaryButtonText = "是",
                    CloseButtonText = "否",
                }) == ContentDialogResult.Primary)
                {
                    Picture = string.Empty;
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
            if (IsBookExisted)
            {
                _navigationService.NavigateWithHierarchy(typeof(EditBook));
                WeakReferenceMessenger.Default.Send(IsbnText,"EditBook");
            }
            else
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
                        Picture = LocalStorage.AddPicture(IsbnText, Picture),
                        ShelfNumber = int.Parse(ShelfNum),
                        IsBorrowed = IsBorrowed,
                    };
                    BooksDb.AddBookAsync(bookInfo);
                    System.Media.SystemSounds.Asterisk.Play();
                    _snackbarService.Show("添加成功", $"书籍《{BookName}》已添加到数据库中。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                    IsbnText = string.Empty;
                    WeakReferenceMessenger.Default.Send(string.Empty, "BookManage");
                }
                else
                {
                    string content = "您必须完善以下书籍信息， 才能将书籍添加到数据库中：\n\n" + tip.ToString();
                    System.Media.SystemSounds.Asterisk.Play();
                    await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                    {
                        Title = "添加书籍",
                        Content = content,
                        CloseButtonText = "去完善",
                    });
                }
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
            Language = string.Empty;
            Picture = string.Empty;
            ShelfNum = string.Empty;
            IsBorrowed = false;
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