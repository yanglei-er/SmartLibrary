using System.Drawing;
using System.Windows.Media.Imaging;
using ViewFaceCore.Model;

namespace Shared.Models
{
    public record class EncodingFace
    {
        public Bitmap FullImage { get; set; }
        public BitmapImage FaceImage { get; set; }
        public FaceInfo FaceInfo { get; set; }
        public FaceMarkPoint[] FaceMarkPoints { get; set; }

        public EncodingFace(Bitmap fullImage, BitmapImage faceImage, FaceInfo faceInfo, FaceMarkPoint[] faceMarkPoints)
        {
            FullImage = fullImage;
            FaceImage = faceImage;
            FaceInfo = faceInfo;
            FaceMarkPoints = faceMarkPoints;
        }
    }
}
