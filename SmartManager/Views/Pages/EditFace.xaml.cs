using SmartManager.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Abstractions.Controls;

namespace SmartManager.Views.Pages
{
    public partial class EditFace : INavigableView<EditFaceViewModel>
    {
        public EditFaceViewModel ViewModel { get; set; }

        public EditFace(EditFaceViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                XuNiBox.Focus();
            }
            else
            {
                ViewModel.IsEditButtonEnabled = true;
            }
        }
    }
}
