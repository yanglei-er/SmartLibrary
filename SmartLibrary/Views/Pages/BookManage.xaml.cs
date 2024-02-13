using SmartLibrary.Models;
using SmartLibrary.ViewModels;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class BookManage : INavigableView<BookManageViewModel>
    {
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex MyRegex();

        private BookInfoSimple bookInfo = new("", "", "", 0, false);

        public BookManageViewModel ViewModel { get; }
        public BookManage(BookManageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void Text_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = MyRegex().IsMatch(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
                bookInfo.Author = dataRowView[2].ToString();
                bookInfo.ShelfNumber = (Int64)dataRowView[11];
                bookInfo.IsBorrowed = Convert.ToBoolean(dataRowView[12]);
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
                else
                {
                    if ((string)dataRowView[1] != bookInfo.BookName || dataRowView[2].ToString() != bookInfo.Author || (Int64)dataRowView[11] != bookInfo.ShelfNumber)
                    {
                        bookInfo.Isbn = (string)dataRowView[0];
                        bookInfo.BookName = (string)dataRowView[1];
                        bookInfo.Author = dataRowView[2].ToString();
                        bookInfo.ShelfNumber = (Int64)dataRowView[11];
                        bookInfo.IsBorrowed = Convert.ToBoolean(dataRowView[12]);
                        ViewModel.Update(bookInfo);
                    }
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem is DataRowView dataRowView)
            {
                bookInfo.Isbn = (string)dataRowView[0];
                bookInfo.BookName = (string)dataRowView[1];
                bookInfo.Author = dataRowView[2].ToString();
                bookInfo.ShelfNumber = (Int64)dataRowView[11];
                bookInfo.IsBorrowed = Convert.ToBoolean(dataRowView[12]);
                ViewModel.Update(bookInfo);
            }
        }
    }
}
