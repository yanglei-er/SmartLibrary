using SmartLibrary.ViewModels;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class BookManage : INavigableView<BookManageViewModel>
    {
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex MyRegex();

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
    }
}
