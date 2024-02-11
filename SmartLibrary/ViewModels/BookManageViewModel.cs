using SmartLibrary.Helpers;
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

        public BookManageViewModel(ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
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

        private void Refresh()
        {
            TotalCount = BooksDb.GetRecordCount();
            TotalPageCount = TotalCount / PageCountList[CurrentIndex] + ((TotalCount % PageCountList[CurrentIndex]) == 0 ? 0 : 1);
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
                    PageButtonList.Add(new PageButton() { Name = i.ToString(), IsCurrentPage = CurrentPage == i });
                }
            }
            else
            {
                if (CurrentPage <= 4)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        PageButtonList.Add(new PageButton() { Name = i.ToString(), IsCurrentPage = CurrentPage == i });
                    }
                    PageButtonList.Add(new PageButton() { Name = "...", IsEnabled = false });
                    PageButtonList.Add(new PageButton() { Name = TotalPageCount.ToString() });
                }
                else if (CurrentPage >= TotalPageCount - 3)
                {
                    PageButtonList.Add(new PageButton() { Name = "1" });
                    PageButtonList.Add(new PageButton() { Name = "...", IsEnabled = false });
                    PageButtonList.Add(new PageButton() { Name = (TotalPageCount - 4).ToString(), IsCurrentPage = CurrentPage == TotalPageCount - 4 });
                    PageButtonList.Add(new PageButton() { Name = (TotalPageCount - 3).ToString(), IsCurrentPage = CurrentPage == TotalPageCount - 3 });
                    PageButtonList.Add(new PageButton() { Name = (TotalPageCount - 2).ToString(), IsCurrentPage = CurrentPage == TotalPageCount - 2 });
                    PageButtonList.Add(new PageButton() { Name = (TotalPageCount - 1).ToString(), IsCurrentPage = CurrentPage == TotalPageCount - 1 });
                    PageButtonList.Add(new PageButton() { Name = TotalPageCount.ToString(), IsCurrentPage = CurrentPage == TotalPageCount });
                }
                else
                {
                    PageButtonList.Add(new PageButton() { Name = "1" });
                    PageButtonList.Add(new PageButton() { Name = "...", IsEnabled = false });
                    PageButtonList.Add(new PageButton() { Name = (CurrentPage - 1).ToString() });
                    PageButtonList.Add(new PageButton() { Name = CurrentPage.ToString(), IsCurrentPage = true });
                    PageButtonList.Add(new PageButton() { Name = (CurrentPage + 1).ToString() });
                    PageButtonList.Add(new PageButton() { Name = "...", IsEnabled = false });
                    PageButtonList.Add(new PageButton() { Name = TotalPageCount.ToString() });
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

            }
            else if (parameter == "Export")
            {

            }
            else if (parameter == "AddBook")
            {

            }
        }

        [RelayCommand]
        private async Task DelBooks(object selectedItems)
        {
            if (selectedItems is IList SelectedItems)
            {
                ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "删除图书",
                    Content = $"是否删除你选择的 {SelectedItems.Count} 本图书",
                    PrimaryButtonText = "是",
                    CloseButtonText = "否",
                });

                if (result == ContentDialogResult.Primary)
                {
                    _snackbarService.Show("删除图书", $"已删除你选择的 {SelectedItems.Count} 本图书", ControlAppearance.Info, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));

                    foreach (DataRowView item in SelectedItems)
                    {
                        string? isbn = item[0].ToString();
                        if (isbn != null)
                        {
                            BooksDb.DelBook(isbn);
                        }
                    }
                    Refresh();
                    Pager();
                }
            }
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

        private void Ds_Rowchanged(object sender, DataRowChangeEventArgs e)
        {
            BooksDb.UpdateDatabase(e.Row.Table);
        }
    }

    public partial class PageButton : ObservableObject
    {
        [ObservableProperty]
        public string _name = string.Empty;

        [ObservableProperty]
        public bool _isCurrentPage = false;

        [ObservableProperty]
        private bool _isEnabled = true;
    }
}
