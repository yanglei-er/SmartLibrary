using SmartLibrary.Helpers;
using System.Data;
using Wpf.Ui;

namespace SmartLibrary.ViewModels
{
    public partial class BookManageViewModel : ObservableObject
    {
        private readonly IContentDialogService _contentDialogService;
        private readonly SQLiteHelper booksDb = SQLiteHelper.GetDatabase("books.smartlibrary");
        private int totalPageCount;

        //[ObservableProperty]
        //private readonly List<int> PageCountList = [20, 30, 50, 80];
        [ObservableProperty]
        private List<int> _pageCountList = [1, 2, 3, 20];

        [ObservableProperty]
        //private ObservableCollection<BookInfo> _dataGridItems = [];
        private DataView _dataGridItems = new();

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private int _currentIndex = 0;

        [ObservableProperty]
        private List<PageButton> _pageButtonList = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _targetPage = 1;

        [ObservableProperty]
        private bool _isFlyoutOpen = false;

        [ObservableProperty]
        private string _flyoutText = string.Empty;

        public BookManageViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
            if (SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
            {
                booksDb.ExecutePagerCompleted += ExecutePagerCompleted;
                booksDb.ExecuteDataTableCompleted += ExecuteDataTableCompleted;

                TotalCount = booksDb.GetRecordCount();
                totalPageCount = TotalCount / PageCountList[CurrentIndex] + ((TotalCount % PageCountList[CurrentIndex]) == 0 ? 0 : 1);

                Refresh();
                ComputeButton();
            }
            else
            {

            }
        }

        ~BookManageViewModel()
        {
            booksDb.ExecutePagerCompleted -= ExecutePagerCompleted;
            booksDb.ExecuteDataTableCompleted -= ExecuteDataTableCompleted;
        }

        private void Refresh()
        {
            booksDb.ExecutePager(CurrentPage, PageCountList[CurrentIndex]);
        }

        private void ComputeButton()
        {
            PageButtonList.Clear();

            // 若页数<=7，那就全显示
            if (totalPageCount <= 7)
            {
                for (int i = 1; i <= totalPageCount; i++)
                {
                    PageButtonList.Add(new PageButton() { Name = i.ToString() });
                }
            }
            else if (totalPageCount > 7)
            {
                PageButtonList.Add(new PageButton() { Name = "1" });
                PageButtonList.Add(new PageButton() { Name = "2" });
                PageButtonList.Add(new PageButton() { Name = "3" });
                PageButtonList.Add(new PageButton() { Name = "..." });
                PageButtonList.Add(new PageButton() { Name = (totalPageCount - 2).ToString() });
                PageButtonList.Add(new PageButton() { Name = (totalPageCount - 1).ToString() });
                PageButtonList.Add(new PageButton() { Name = totalPageCount.ToString() });
            }
        }

        partial void OnCurrentIndexChanged(int value)
        {
            totalPageCount = TotalCount / PageCountList[value] + ((TotalCount % PageCountList[value]) == 0 ? 0 : 1);
            if (CurrentPage == 1) Refresh();
            CurrentPage = 1;
            ComputeButton();
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
            if (parameter == "HomePage")
            {
                CurrentPage = 1;
            }
            else if (parameter == "PageUp")
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                }
            }
            else if (parameter == "PageDown")
            {
                if (CurrentPage < totalPageCount)
                {
                    CurrentPage++;
                }
            }
            else if (parameter == "EndPage")
            {
                CurrentPage = totalPageCount;
            }
        }

        partial void OnCurrentPageChanged(int value)
        {
            TargetPage = value;
            Refresh();
        }

        [RelayCommand]
        private void GotoPage(string page)
        {
            PageButtonList.Remove(PageButtonList[0]);
            MessageBox.Show(PageButtonList.Count().ToString());
        }

        [RelayCommand]
        partial void OnTargetPageChanged(int value)
        {
            if (value > totalPageCount)
            {
                TargetPage = totalPageCount;
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
    }
}
