using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SmartLibrary.Helpers;
using SmartLibrary.Models;
using SmartLibrary.Views.Pages;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartLibrary.ViewModels
{
    public partial class BookManageViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
        private int TotalPageCount;

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
            if (SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
            {
                BooksDb.ExecutePagerCompleted += ExecutePagerCompleted;
                Refresh();
                Pager();
            }
            else
            {

            }
        }

        ~BookManageViewModel()
        {
            BooksDb.ExecutePagerCompleted -= ExecutePagerCompleted;
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
            string? isbn = selectedItem[0].ToString();
            if (isbn != null)
            {
                WeakReferenceMessenger.Default.Send(isbn);
            }
        }

        private void Refresh()
        {
            TotalCount = BooksDb.GetRecordCount();
            TotalPageCount = TotalCount / PageCountList[CurrentIndex] + ((TotalCount % PageCountList[CurrentIndex]) == 0 ? 0 : 1);
            if (CurrentPage > TotalPageCount) CurrentPage = TotalPageCount;
            if (TotalPageCount == 1) { IsPageUpEnabled = false; IsPageDownEnabled = false; return; }
            if (CurrentPage != 1) { IsPageUpEnabled = true; }
            if (CurrentPage != TotalPageCount) { IsPageDownEnabled = true; }
        }

        private void Pager()
        {
            IsDelButtonEnabled = false;
            BooksDb.ExecutePager(CurrentPage, PageCountList[CurrentIndex]);

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
            Refresh();
            if (CurrentPage == 1) Pager();
            CurrentPage = 1;
        }

        [RelayCommand]
        private void OnTopButtonClick(string parameter)
        {
            if (parameter == "Import")
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "导入数据库",
                    Filter = "SmartLibrary数据库 (*.smartlibrary)|*.smartlibrary",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Multiselect = true,
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    int[] mergedResult = [0, 0];
                    string[] fileNames = openFileDialog.FileNames;
                    foreach (string fileName in fileNames)
                    {
                        int[] _mergedResult = BooksDb.MergeDatabase(fileName);
                        mergedResult[0] += _mergedResult[0];
                        mergedResult[1] += _mergedResult[1];
                    }
                    Refresh();
                    Pager();
                    _snackbarService.Show("导入数据库", $"{fileNames.Length} 个数据库已导入，共 {mergedResult[0] + mergedResult[1]} 条数据，导入 {mergedResult[0]} 条，重复 {mergedResult[1]} 条。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
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
                    System.IO.File.Copy(@".\database\books.smartlibrary", saveFileDialog.FileName, true);
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
                foreach (DataRowView item in selectedItems)
                {
                    string? isbn = item[0].ToString();
                    if (isbn != null)
                    {
                        BooksDb.DelBook(isbn);
                    }
                }
                Refresh();
                Pager();
                _snackbarService.Show("删除图书", $"已删除你选择的 {selectedItems.Count} 本图书", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private void DelOneBook(DataRowView selectedItem)
        {
            string? isbn = selectedItem[0].ToString();
            if (isbn != null)
            {
                BooksDb.DelBook(isbn);
            }
            Refresh();
            Pager();
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
            Pager();
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
                TargetPage = TotalPageCount;
                FlyoutText = "输入页码超过最大页码！";
                IsFlyoutOpen = true;
            }
            else if (value == 0)
            {
                TargetPage = 1;
                FlyoutText = "最小页码为 1 ";
                IsFlyoutOpen = true;
            }
            else
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

        private void ExecutePagerCompleted(DataTable datatable)
        {
            DataGridItems = datatable.DefaultView;
        }

        public void Update(BookInfoSimple bookInfo)
        {
            BooksDb.UpdateSimple(bookInfo.Isbn, bookInfo.BookName, bookInfo.Author, bookInfo.ShelfNumber, bookInfo.IsBorrowed);
        }
    }

    public record class PageButton
    {
        public string Name { get; init; }
        public bool IsCurrentPage { get; init; }
        public bool IsEnabled { get; init; }

        public PageButton(string name, bool isCurrentPage = false, bool isEnabled = true)
        {
            Name = name;
            IsCurrentPage = isCurrentPage;
            IsEnabled = isEnabled;
        }
    }
}
