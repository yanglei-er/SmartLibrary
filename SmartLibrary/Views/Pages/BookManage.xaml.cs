using SmartLibrary.ViewModels;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class BookManage : INavigableView<BookManageViewModel>
    {
        public BookManageViewModel ViewModel { get; }
        public BookManage(BookManageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void Text_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.GotoTargetPage(textBox.Text);
                XuNiBox.Focus();
            }
        }
    }
}
