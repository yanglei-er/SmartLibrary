using CommunityToolkit.Mvvm.Messaging;
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
        private readonly SQLiteHelper BooksDb = SQLiteHelper.GetDatabase("books.smartlibrary");

        private int TotalPageCount;
        private bool needRefresh = false;

        [ObservableProperty]
        private ObservableCollection<BookShelfInfo> bookListItems = [];

        public BookshelfViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            WeakReferenceMessenger.Default.Register<string, string>(this, "Bookshelf", OnMessageReceived);

            if (SQLiteHelper.IsDatabaseConnected("books.smartlibrary"))
            {
                needRefresh = true;
            }
        }

        private void OnMessageReceived(object recipient, string message)
        {

        }

        public void OnNavigatedTo()
        {
            if (needRefresh)
            {
                AddBookToList();
                needRefresh = false;
            }
        }

        public void OnNavigatedFrom()
        {

        }

        private async void AddBookToList()
        {
            BookListItems.Clear();
            foreach (DataRow row in (await BooksDb.GetBookList(1, 20)).Rows)
            {
                BookShelfInfo book = new((string)row[0], (string)row[1], (string)row[2], row[3].ToString() ?? string.Empty, row[4].ToString() ?? string.Empty);
                BookListItems.Add(book);
            }
        }

        [RelayCommand]
        private void BookInfo(string isbn)
        {
            //_navigationService.Navigate(typeof(Views.Pages.BookInfo));
            //WeakReferenceMessenger.Default.Send(isbn, "BookInfo");
            _navigationService.NavigateWithHierarchy(typeof(EditBook));
            WeakReferenceMessenger.Default.Send(isbn, "EditBook");
        }
    }
}