using SmartLibrary.Models;
using SmartLibrary.ViewModels;
using System.Data;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class BookManage : INavigableView<BookManageViewModel>
    {
        private readonly BookInfoSimple bookInfo = new("", "", "", 0, false);

        public BookManageViewModel ViewModel { get; }

        public BookManage(BookManageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.GotoTargetPage(textBox.Text);
                XuNiBox.Focus();
            }
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataGrid.SelectedItem == null) { DelButton.IsEnabled = false; }
            else { DelButton.IsEnabled = true; }
        }

        private void DataGrid_BeginningEdit(object sender, System.Windows.Controls.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is DataRowView dataRowView)
            {
                bookInfo.Isbn = (string)dataRowView[0];
                bookInfo.BookName = (string)dataRowView[1];
                bookInfo.Author = (string)dataRowView[2];
                bookInfo.ShelfNumber = (long)dataRowView[3];
                bookInfo.IsBorrowed = Convert.ToBoolean(dataRowView[4]);
            }
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.Item is DataRowView dataRowView)
            {
                if (string.IsNullOrEmpty(dataRowView[1].ToString()))
                {
                    dataRowView[1] = bookInfo.BookName;
                }
                if (string.IsNullOrEmpty(dataRowView[2].ToString()))
                {
                    dataRowView[2] = bookInfo.Author;
                }
                else
                {
                    if ((string)dataRowView[1] != bookInfo.BookName || (string)dataRowView[2] != bookInfo.Author || (long)dataRowView[3] != bookInfo.ShelfNumber)
                    {
                        bookInfo.Isbn = (string)dataRowView[0];
                        bookInfo.BookName = (string)dataRowView[1];
                        bookInfo.Author = (string)dataRowView[2];
                        bookInfo.ShelfNumber = (long)dataRowView[3];
                        bookInfo.IsBorrowed = Convert.ToBoolean(dataRowView[4]);
                        ViewModel.UpdateSimple(bookInfo);
                    }
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem is DataRowView dataRowView)
            {
                string isbn = (string)dataRowView[0];
                bool isBorrowed = Convert.ToBoolean(dataRowView[4]);
                ViewModel.CheckBox_Click(isbn, isBorrowed);
            }
        }
    }
}