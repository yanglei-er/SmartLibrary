using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SmartLibrary.Helpers;
using SmartLibrary.Models;
using SmartLibrary.Views.Pages;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartLibrary.ViewModels
{
    public partial class BookManageViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
        private int TotalPageCount;
        private bool needRefresh = false;

        [ObservableProperty]
        private string _autoSuggestBoxText = string.Empty;

        [ObservableProperty]
        private bool _missingDatabase = false;

        [ObservableProperty]
        private bool _databaseEmpty = false;

        [ObservableProperty]
        private bool _isTopbarEnabled = true;

        [ObservableProperty]
        private bool _isBottombarEnabled = true;

        [ObservableProperty]
        private bool _isDelButtonEnabled = false;

        [ObservableProperty]
        private List<int> _pageCountList = [20, 30, 50, 80];

        [ObservableProperty]
        private DataView _dataGridItems = new();

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private int _currentIndex = 0;

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

        public BookManageViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            WeakReferenceMessenger.Default.Register<string, string>(this, "BookManage", (_, _) => needRefresh = true);

            if (SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
            {
                needRefresh = true;
            }
            else
            {
                MissingDatabase = true;
                IsTopbarEnabled = false;
                IsBottombarEnabled = false;
            }
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

        [RelayCommand]
        private void RefreshDatabase()
        {
            BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
            if (SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
            {
                MissingDatabase = false;
                IsTopbarEnabled = true;
                RefreshAsync();
                PagerAsync();
            }
        }

        [RelayCommand]
        private async Task CreateDatabase()
        {
            await BooksDb.CreateDataBaseAsync();
            BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
            MissingDatabase = false;
            IsTopbarEnabled = true;
            RefreshAsync();
        }

        [RelayCommand]
        private void AddBook()
        {
            _navigationService.NavigateWithHierarchy(typeof(AddBook));
        }

        [RelayCommand]
        private void EditBook(DataRowView selectedItem)
        {
            _navigationService.NavigateWithHierarchy(typeof(EditBook));
            string isbn = (string)selectedItem[0];
            WeakReferenceMessenger.Default.Send(isbn, "EditBook");
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
            TotalPageCount = TotalCount / PageCountList[CurrentIndex] + ((TotalCount % PageCountList[CurrentIndex]) == 0 ? 0 : 1);
            if (CurrentPage > TotalPageCount) CurrentPage = TotalPageCount;
            if (TotalPageCount == 1) { IsPageUpEnabled = false; IsPageDownEnabled = false; return; }
            if (CurrentPage != 1) { IsPageUpEnabled = true; }
            if (CurrentPage != TotalPageCount) { IsPageDownEnabled = true; }
        }

        private async void PagerAsync()
        {
            IsDelButtonEnabled = false;
            DataGridItems = (await BooksDb.ExecutePagerSimpleAsync(CurrentPage, PageCountList[CurrentIndex])).DefaultView;

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

        partial void OnCurrentIndexChanged(int value)
        {
            RefreshAsync();
            if (CurrentPage == 1) PagerAsync();
            CurrentPage = 1;
        }

        [RelayCommand]
        private async Task OnTopButtonClick(string parameter)
        {
            if (parameter == "Import")
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "导入数据库",
                    Filter = "SmartLibrary数据库 (*.smartlibrary)|*.smartlibrary",
                    Multiselect = true,
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    if (!SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
                    {
                        await CreateDatabase();
                    }
                    int[] mergedResult = [0, 0];
                    List<string> fileNames = new(openFileDialog.FileNames);
                    List<string> repeatFileNames = [];
                    foreach (string fileName in fileNames)
                    {
                        if (fileName == Path.GetFullPath(@".\database\books.smartlibrary")) //避免重复
                        {
                            repeatFileNames.Add(fileName);
                            continue;
                        }
                        int[] _mergedResult = await BooksDb.MergeDatabaseAsync(fileName);
                        mergedResult[0] += _mergedResult[0];
                        mergedResult[1] += _mergedResult[1];
                    }
                    foreach (string fileName in repeatFileNames)
                    {
                        fileNames.Remove(fileName);
                        mergedResult[1] += 1;
                    }
                    if (mergedResult[0] != 0)
                    {
                        RefreshAsync();
                        PagerAsync();
                        WeakReferenceMessenger.Default.Send("refresh", "Bookshelf");
                    }
                    _snackbarService.Show("导入数据库", $"{fileNames.Count} 个数据库已导入，共 {mergedResult[0] + mergedResult[1]} 条数据，导入 {mergedResult[0]} 条，重复 {mergedResult[1]} 条。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
            }
            else if (parameter == "Export")
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Title = "导出数据库",
                    Filter = "SmartLibrary数据库 (*.smartlibrary)|*.smartlibrary",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(@".\database\books.smartlibrary", saveFileDialog.FileName, true);
                    _snackbarService.Show("导出数据库", $"{System.IO.Path.GetFileName(saveFileDialog.FileName)} 已导出至 {System.IO.Path.GetDirectoryName(saveFileDialog.FileName)} 下", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
            }
        }

        [RelayCommand]
        private async Task DelBooks(IList selectedItems)
        {
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
            {
                Title = "删除图书",
                Content = $"是否删除你选择的 {selectedItems.Count} 本图书，此操作不可撤销！",
                PrimaryButtonText = "是",
                CloseButtonText = "否",
            });

            if (result == ContentDialogResult.Primary)
            {
                List<int> indexs = [];
                if (DataGridItems.Table != null)
                {
                    foreach (DataRowView item in selectedItems)
                    {
                        string isbn = (string)item[0];
                        BooksDb.DelBookAsync(isbn);
                        if (!IsBottombarEnabled)
                        {
                            indexs.Add(DataGridItems.Table.Rows.IndexOf(item.Row));
                        }
                    }
                }

                _snackbarService.Show("删除图书", $"已删除你选择的 {selectedItems.Count} 本图书", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));

                if (IsBottombarEnabled)
                {
                    RefreshAsync();
                    PagerAsync();
                }
                else
                {
                    if (DataGridItems.Table != null)
                    {
                        foreach (int index in indexs)
                        {
                            DataGridItems.Table.Rows.RemoveAt(index);
                        }
                        DataGridItems.Table.AcceptChanges();
                    }
                    if (DataGridItems.Count == 0)
                    {
                        DatabaseEmpty = true;
                    }
                    TotalCount = DataGridItems.Count;
                }
            }
            WeakReferenceMessenger.Default.Send("refresh", "Bookshelf");
            WeakReferenceMessenger.Default.Send("refresh", "BookInfo");
            WeakReferenceMessenger.Default.Send("refresh", "Borrow_Return_Book");
        }

        [RelayCommand]
        private void DelOneBook(DataRowView selectedItem)
        {
            string isbn = (string)selectedItem[0];
            BooksDb.DelBookAsync(isbn);
            if (IsBottombarEnabled)
            {
                RefreshAsync();
                PagerAsync();
            }
            else
            {
                if (DataGridItems.Table != null)
                {
                    DataGridItems.Table.Rows.Remove(selectedItem.Row);
                    DataGridItems.Table.AcceptChanges();
                }
                if (DataGridItems.Count == 0)
                {
                    DatabaseEmpty = true;
                }
                TotalCount--;
            }
            WeakReferenceMessenger.Default.Send("refresh", "Bookshelf");
            WeakReferenceMessenger.Default.Send("." + isbn, "BookInfo");
            WeakReferenceMessenger.Default.Send("." + isbn, "Borrow_Return_Book");
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

        public void UpdateSimple(BookInfoSimple bookInfo)
        {
            BooksDb.UpdateSimpleAsync(bookInfo.Isbn, bookInfo.BookName, bookInfo.Author, bookInfo.ShelfNumber, bookInfo.IsBorrowed);
            WeakReferenceMessenger.Default.Send("refresh", "Bookshelf");
            WeakReferenceMessenger.Default.Send("." + bookInfo.Isbn, "BookInfo");
            WeakReferenceMessenger.Default.Send("." + bookInfo.Isbn, "Borrow_Return_Book");
        }

        public void CheckBox_Click(string isbn, bool value)
        {
            if (value)
            {
                BooksDb.BorrowBookAsync(isbn);
            }
            else
            {
                BooksDb.ReturnBookAsync(isbn);
            }
            WeakReferenceMessenger.Default.Send("." + isbn, "BookInfo");
            WeakReferenceMessenger.Default.Send("." + isbn, "Borrow_Return_Book");
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
                if (int.TryParse(value, out int num))
                {
                    DataGridItems = (await BooksDb.AutoSuggestByNumAsync(num)).DefaultView;
                }
                else
                {
                    DataGridItems = (await BooksDb.AutoSuggestByStringAsync(value)).DefaultView;
                }
                if (DataGridItems.Count > 0)
                {
                    DatabaseEmpty = false;
                }
                else
                {
                    DatabaseEmpty = true;
                }
                TotalCount = DataGridItems.Count;
            }
            else
            {
                RefreshAsync();
                PagerAsync();
            }
        }
    }
}