using SmartManager.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartManager.Views.Pages
{
    public partial class AddFace : INavigableView<AddFaceViewModel>
    {
        public AddFaceViewModel ViewModel { get; set; }
        public AddFace(AddFaceViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                XuNiBox.Focus();
            }
        }

        private void OpenCamera_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.OpenCamera(CameraImage);
        }
    }
}
