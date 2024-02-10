using SmartLibrary.Helpers;
using System.Collections.ObjectModel;
using System.Data;
using Wpf.Ui;

namespace SmartLibrary.ViewModels
{
    public partial class BookManageViewModel : ObservableObject
    {
        private readonly IContentDialogService _contentDialogService;
        private readonly SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
        private int TotalPageCount;

        [ObservableProperty]
        private List<int> _pageCountList = [20, 30, 50, 80];

        [ObservableProperty]
        //private ObservableCollection<BookInfo> _dataGridItems = [];
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

        public BookManageViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
            if (SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
            {
                BooksDb.ExecutePagerCompleted += ExecutePagerCompleted;
                BooksDb.ExecuteDataTableCompleted += ExecuteDataTableCompleted;

                TotalCount = BooksDb.GetRecordCount();
                TotalPageCount = TotalCount / PageCountList[CurrentIndex] + ((TotalCount % PageCountList[CurrentIndex]) == 0 ? 0 : 1);
                if (TotalPageCount > 1) IsPageDownEnabled = true;

                Refresh();
                ComputeButton();
            }
            else
            {

            }
        }

        ~BookManageViewModel()
        {
            BooksDb.ExecutePagerCompleted -= ExecutePagerCompleted;
            BooksDb.ExecuteDataTableCompleted -= ExecuteDataTableCompleted;
        }

        private void Refresh()
        {
            BooksDb.ExecutePager(CurrentPage, PageCountList[CurrentIndex]);
        }

        private void ComputeButton()
        {
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
            TotalPageCount = TotalCount / PageCountList[value] + ((TotalCount % PageCountList[value]) == 0 ? 0 : 1);
            if (CurrentPage == 1) Refresh();
            CurrentPage = 1;
            ComputeButton();
            if (TotalPageCount == 1) { IsPageUpEnabled = false; IsPageDownEnabled = false; }
            else { IsPageDownEnabled = true; }
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
            else if (parameter == "DelBook")
            {

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
            Refresh();
            ComputeButton();
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

        private void ExecuteDataTableCompleted(DataTable datatable)
        {

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
