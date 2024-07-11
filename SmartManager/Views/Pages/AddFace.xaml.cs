using SmartManager.ViewModels;
using System.Windows.Controls;
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                XuNiBox.Focus();
            }
        }

        private void OpenCameraClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.OpenCamera(CameraImage, MaskImage);
        }

        private void CaptureFaceClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.CaptureFace(CameraImage);
        }
    }
}
