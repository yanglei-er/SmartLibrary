using SmartLibrary.Helpers;
using System.Data;

namespace SmartLibrary.ViewModels
{
    public partial class BookManageViewModel : ObservableObject
    {
        private readonly SQLiteHelper booksDb = new SQLiteHelper("books.smartlibrary");

        [ObservableProperty]
        private string _networkStateImageSource = "pack://application:,,,/Assets/pic/red.png";

        [ObservableProperty]
        private string _databaseStateImageSource = "pack://application:,,,/Assets/pic/red.png";

        [ObservableProperty]
        //private ObservableCollection<BookInfo> _dataGridItems = [];
        private DataView _dataGridItems = new DataView();

        public BookManageViewModel()
        {
            if (Network.IsInternetConnected())
            {
                NetworkStateImageSource = "pack://application:,,,/Assets/pic/green.png";
            }
            if (SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
            {
                DatabaseStateImageSource = "pack://application:,,,/Assets/pic/green.png";
                //SQLiteHelper.DataBaceList.Add("MAIN", booksDb);
                //string cmdText = "SELECT * FROM main";
                //DataTable dt = booksDb.ExecuteDataTable(cmdText, null);
                //DataGridItems = dt.DefaultView;
            }
        }

        [RelayCommand]
        private void OnButtonClick(string parameter)
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
    }
}
