using SmartManager.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartManager.Views.Pages
{
    public partial class FaceManage : INavigableView<FaceManageViewModel>
    {
        public FaceManageViewModel ViewModel { get; }

        public FaceManage(FaceManageViewModel viewModel)
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

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                XuNiBox.Focus();
            }
        }
    }
}
