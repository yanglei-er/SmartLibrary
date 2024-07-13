using System.Drawing;
using ViewFaceCore.Model;

namespace Shared.Models
{
    public record class Face
    {
        public Bitmap FaceImage { get; set; }
        public FaceInfo FaceInfo { get; set; }
        public FaceMarkPoint[] FaceMarkPoints { get; set; }

        public Face(Bitmap faceImage, FaceInfo faceInfo, FaceMarkPoint[] faceMarkPoints)
        {
            FaceImage = faceImage;
            FaceInfo = faceInfo;
            FaceMarkPoints = faceMarkPoints;
        }
    }
}
