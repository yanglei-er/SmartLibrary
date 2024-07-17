using Shared.Helpers;
using SmartManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace SmartManager.Views.Pages
{
    public partial class AddFace : INavigableView<AddFaceViewModel>
    {
        public AddFaceViewModel ViewModel { get; set; }
        private ScrollViewer? scroll;

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

        private void OpenCameraClick(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenCamera(CameraImage, MaskImage);
        }

        private void CaptureFaceClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CaptureFace(CameraImage);
        }

        private void AddFaceClick(object sender, RoutedEventArgs e)
        {
            ViewModel.AddFace();
        }

        private void FaceListView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (scroll == null)
            {
                scroll = Utils.FindVisualChild<ScrollViewer>(FaceListView);
            }
            else
            {
                if (e.Delta < 0)
                {
                    scroll.LineRight();
                }
                else
                {
                    scroll.LineLeft();
                }
                scroll.ScrollToTop();
            }
        }

        private void DrawFaceRectangle_Unchecked(object sender, RoutedEventArgs e)
        {
            if(!FaceComparison_CheckBox.IsChecked ?? false)
            {
                BitmapSource mask = BitmapImage.Create(2, 2, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.Transparent }), new byte[] { 0, 0, 0, 0 }, 1);
                MaskImage.Source = mask;
            }
            
        }

        private void FaceComparison_Unchecked(object sender, RoutedEventArgs e)
        {
            if(!DrawFaceRectangle_CheckBox.IsChecked ?? false)
            {
                BitmapSource mask = BitmapImage.Create(2, 2, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.Transparent }), new byte[] { 0, 0, 0, 0 }, 1);
                MaskImage.Source = mask;
            }
        }

        private void DeviceComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ViewModel.DeviceComboBox_DropDownOpened();
        }
    }
}
