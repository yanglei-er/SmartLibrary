using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmartManager.Models
{
    public record class FaceImage
    {
        public BitmapImage? ImageSource { get; set; }

        public FaceImage(BitmapImage? imageSource)
        {
            ImageSource = imageSource;
        }
    }
}
