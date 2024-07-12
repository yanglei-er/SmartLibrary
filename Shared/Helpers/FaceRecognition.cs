using Hompus.VideoInputDevices;
using OpenCvSharp;
using System.Drawing;
using ViewFaceCore;
using ViewFaceCore.Model;

namespace Shared.Helpers
{
    public static class FaceRecognition
    {
        private static VideoCapture videoCapture = new();
        private static readonly ViewFaceCore.Core.FaceDetector faceDetector = new();
        private static readonly Pen pen = new(Color.Red, 2);

        public static bool IsCameraOpened { get; set; }

        public static int SleepTime
        {
            get
            {
                return (int)Math.Round(1000 / videoCapture.Fps);
            }
        }

        public static Dictionary<string, int> SystemCameraDevices
        {
            get
            {
                using SystemDeviceEnumerator sde = new();
                Dictionary<string, int> devices = [];
                IReadOnlyDictionary<int, string> systemDevices;
                try
                {
                    systemDevices = sde.ListVideoInputDevice();
                    foreach (var device in systemDevices)
                    {
                        devices.Add(device.Value, device.Key);
                    }
                }
                catch
                {
                    devices.Add("暂无摄像头", -1);
                }
                return devices;
            }
        }

        static FaceRecognition()
        {

        }

        public static Image GetImage()
        {
            using Mat v_mat = new();
            videoCapture.Read(v_mat);
            if (v_mat != null)
            {
                return Image.FromStream(v_mat.ToMemoryStream());
            }
            else
            {
                return new Bitmap(1, 1);
            }
        }

        public static Bitmap GetMaskImage(Image cameraImage)
        {
            Bitmap mask = new(cameraImage.Width, cameraImage.Height);
            using Graphics maskGraphics = Graphics.FromImage(mask);

            FaceInfo[] faceInfos = faceDetector.Detect(cameraImage);
            foreach (FaceInfo face in faceInfos)
            {
                maskGraphics.DrawRectangle(pen, face.Location.X, face.Location.Y, face.Location.Width, face.Location.Height);
            }
            return mask;
        }

        public async static ValueTask<Bitmap> GetFace(Image cameraImage)
        {
            FaceInfo[] faceInfos = await faceDetector.DetectAsync(cameraImage);

            if (faceInfos.Length > 0)
            {
                FaceInfo face = faceInfos[0];
                Rectangle cropArea = new(face.Location.X, face.Location.Y, face.Location.Width, face.Location.Height);
                using Bitmap bitmap = (Bitmap)cameraImage;
                using Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
                Bitmap a = ImageProcess.Resize(croppedBitmap, 164, 216);
                return a;
            }
            else
            {
                Bitmap bitmap = new(1, 1);
                return bitmap;
            }
        }

        public static bool OpenCamera(int videoCapture_id, out string message)
        {
            videoCapture = new(videoCapture_id);
            videoCapture.Set(VideoCaptureProperties.FrameWidth, 720);
            videoCapture.Set(VideoCaptureProperties.FrameHeight, 480);//高度
            videoCapture.Set(VideoCaptureProperties.Fps, 10);
            try
            {
                message = string.Empty;
                if (videoCapture.IsOpened())
                {
                    IsCameraOpened = true;
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                {
                    message = e.Message;
                    return false;
                }
            }
        }

        public static void CloseCamera()
        {
            if (videoCapture.IsOpened())
            {
                IsCameraOpened = false;
                videoCapture.Release();
            }
        }
    }
}
