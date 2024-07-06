using System.Windows.Media.Imaging;

namespace SmartManager.Models
{
    public record class FaceImage
    {
        public BitmapSource ImageSource { get; set; }

        public FaceImage(BitmapSource imageSource)
        {
            ImageSource = imageSource;
        }
    }
}
