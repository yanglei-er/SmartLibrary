using CommunityToolkit.Mvvm.Messaging;
using Shared.Helpers;
using Shared.Models;
using SmartLibrary.Helpers;
using SmartLibrary.Models;
using SmartLibrary.Views.Pages;
using System.Collections.ObjectModel;
using System.Data;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartLibrary.ViewModels
{
    public partial class BookshelfViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly Database BooksDb = Database.GetDatabase("books.smartlibrary");
        private int TotalPageCount;
        private bool needRefresh = false;

        [ObservableProperty]
        private string _autoSuggestBoxText = string.Empty;

        [ObservableProperty]
        private bool _missingDatabase = false;

        [ObservableProperty]
        private bool _databaseEmpty = false;

        [ObservableProperty]
        private bool _isBottombarEnabled = true;

        [ObservableProperty]
        private List<int> _pageCountList = [20, 30, 50, 80];

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private int _displayIndex = int.Parse(SettingsHelper.GetConfig("BookshelfDisplayIndex"));

        [ObservableProperty]
        private ObservableCollection<PageButton> _pageButtonList = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _targetPage = 1;

        [ObservableProperty]
        private bool _isPageUpEnabled = false;

        [ObservableProperty]
        private bool _isPageDownEnabled = false;

        [ObservableProperty]
        private bool _isFlyoutOpen = false;

        [ObservableProperty]
        private string _flyoutText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<BookShelfInfo> bookListItems = [];

        public BookshelfViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            WeakReferenceMessenger.Default.Register<string, string>(this, "Bookshelf", OnMessageReceived);

            if (Database.IsDatabaseConnected("books.smartlibrary"))
            {
                needRefresh = true;
            }
            else
            {
                MissingDatabase = true;
                IsBottombarEnabled = false;
            }

            _navigationService.Navigate(typeof(Borrow_Return_Book));
            _navigationService.GoBack();
        }

        private void OnMessageReceived(object recipient, string message)
        {
            if (message == "databaseCreat")
            {
                MissingDatabase = false;
            }
            needRefresh = true;
        }

        public void OnNavigatedTo()
        {
            if (needRefresh)
            {
                if (string.IsNullOrEmpty(AutoSuggestBoxText))
                {
                    RefreshAsync();
                    PagerAsync();
                }
                else
                {
                    AutoSuggest(AutoSuggestBoxText);
                }
                needRefresh = false;
            }
        }

        public void OnNavigatedFrom()
        {

        }

        private async void RefreshAsync()
        {
            TotalCount = await BooksDb.GetRecordCountAsync();
            if (TotalCount == 0)
            {
                DatabaseEmpty = true;
                IsBottombarEnabled = false;
                TotalPageCount = 0;
                return;
            }
            else
            {
                DatabaseEmpty = false;
                IsBottombarEnabled = true;
            }
            TotalPageCount = TotalCount / PageCountList[DisplayIndex] + ((TotalCount % PageCountList[DisplayIndex]) == 0 ? 0 : 1);
            if (CurrentPage > TotalPageCount) CurrentPage = TotalPageCount;
            if (TotalPageCount == 1) { IsPageUpEnabled = false; IsPageDownEnabled = false; return; }
            if (CurrentPage != 1) { IsPageUpEnabled = true; }
            if (CurrentPage != TotalPageCount) { IsPageDownEnabled = true; }
        }

        private async void PagerAsync()
        {
            BookListItems.Clear();
            foreach (DataRow row in (await BooksDb.GetBookList(CurrentPage, PageCountList[DisplayIndex])).Rows)
            {
                BookShelfInfo book = new((string)row[0], (string)row[1], (string)row[2], row[3].ToString() ?? string.Empty, row[4].ToString() ?? string.Empty);
                BookListItems.Add(book);
            }

            PageButtonList.Clear();
            if (TotalPageCount <= 7)
            {
                for (int i = 1; i <= TotalPageCount; i++)
                {
                    PageButtonList.Add(new PageButton(i.ToString(), CurrentPage == i));
                }
            }
            else
            {
                if (CurrentPage <= 4)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        PageButtonList.Add(new PageButton(i.ToString(), CurrentPage == i));
                    }
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton(TotalPageCount.ToString()));
                }
                else if (CurrentPage >= TotalPageCount - 3)
                {
                    PageButtonList.Add(new PageButton("1"));
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton((TotalPageCount - 4).ToString(), CurrentPage == TotalPageCount - 4));
                    PageButtonList.Add(new PageButton((TotalPageCount - 3).ToString(), CurrentPage == TotalPageCount - 3));
                    PageButtonList.Add(new PageButton((TotalPageCount - 2).ToString(), CurrentPage == TotalPageCount - 2));
                    PageButtonList.Add(new PageButton((TotalPageCount - 1).ToString(), CurrentPage == TotalPageCount - 1));
                    PageButtonList.Add(new PageButton(TotalPageCount.ToString(), CurrentPage == TotalPageCount));
                }
                else
                {
                    PageButtonList.Add(new PageButton("1"));
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton((CurrentPage - 1).ToString()));
                    PageButtonList.Add(new PageButton(CurrentPage.ToString(), true));
                    PageButtonList.Add(new PageButton((CurrentPage + 1).ToString()));
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton(TotalPageCount.ToString()));
                }
            }
        }

        partial void OnDisplayIndexChanged(int value)
        {
            SettingsHelper.SetConfig("BookshelfDisplayIndex", value.ToString());
            RefreshAsync();
            if (CurrentPage == 1) PagerAsync();
            CurrentPage = 1;
        }

        [RelayCommand]
        private void OnPageButtonClick(string parameter)
        {
            if (parameter == "PageUp")
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                    if (!IsPageDownEnabled) IsPageDownEnabled = true;
                }
            }
            else
            {
                if (CurrentPage < TotalPageCount)
                {
                    CurrentPage++;
                    if (!IsPageUpEnabled) IsPageUpEnabled = true;
                }
            }
        }

        partial void OnCurrentPageChanged(int value)
        {
            TargetPage = value;
            PagerAsync();
            if (CurrentPage == 1) IsPageUpEnabled = false;
            else if (CurrentPage == TotalPageCount) IsPageDownEnabled = false;
        }

        [RelayCommand]
        private void GotoPage(string page)
        {
            CurrentPage = int.Parse(page);
            if (CurrentPage > 1) IsPageUpEnabled = true;
            if (CurrentPage < TotalPageCount) IsPageDownEnabled = true;
        }

        [RelayCommand]
        partial void OnTargetPageChanged(int value)
        {
            if (value > TotalPageCount)
            {
                FlyoutText = $"输入页码超过最大页码！";
                IsFlyoutOpen = true;
                TargetPage = TotalPageCount;
            }
            else if (value == 0)
            {
                FlyoutText = "最小页码为 1 ";
                IsFlyoutOpen = true;
                TargetPage = 1;
            }
            else if (value > 0 && value < TotalPageCount)
            {
                if (IsFlyoutOpen)
                {
                    IsFlyoutOpen = false;
                }
            }
        }

        public void GotoTargetPage(string page)
        {
            if (string.IsNullOrEmpty(page))
            {
                TargetPage = -1;
                TargetPage = CurrentPage;
            }
            else
            {
                CurrentPage = TargetPage;
                if (CurrentPage > 1) IsPageUpEnabled = true;
                if (CurrentPage < TotalPageCount) IsPageDownEnabled = true;
            }
            if (IsFlyoutOpen)
            {
                IsFlyoutOpen = false;
            }
        }

        partial void OnAutoSuggestBoxTextChanged(string value)
        {
            AutoSuggest(value);
        }

        private async void AutoSuggest(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                IsBottombarEnabled = false;
                BookListItems.Clear();
                if (int.TryParse(value, out int num))
                {
                    foreach (DataRow row in (await BooksDb.AutoSuggestBookShelfInfoByNumAsync(num)).Rows)
                    {
                        BookShelfInfo book = new((string)row[0], (string)row[1], (string)row[2], row[3].ToString() ?? string.Empty, row[4].ToString() ?? string.Empty);
                        BookListItems.Add(book);
                    }
                }
                else
                {
                    foreach (DataRow row in (await BooksDb.AutoSuggestBookShelfInfoByStringAsync(value)).Rows)
                    {
                        BookShelfInfo book = new((string)row[0], (string)row[1], (string)row[2], row[3].ToString() ?? string.Empty, row[4].ToString() ?? string.Empty);
                        BookListItems.Add(book);
                    }
                }
                if (BookListItems.Count > 0)
                {
                    DatabaseEmpty = false;
                }
                else
                {
                    DatabaseEmpty = true;
                }
                TotalCount = BookListItems.Count;
            }
            else
            {
                RefreshAsync();
                PagerAsync();
            }
        }

        [RelayCommand]
        private void BookInfo(string isbn)
        {
            _navigationService.Navigate(typeof(Views.Pages.BookInfo));
            WeakReferenceMessenger.Default.Send(isbn, "BookInfo");
            WeakReferenceMessenger.Default.Send(isbn, "Borrow_Return_Book");
        }
    }
}