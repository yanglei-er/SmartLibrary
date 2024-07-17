using Hompus.VideoInputDevices;
using Newtonsoft.Json;
using OpenCvSharp;
using Shared.Models;
using System.Drawing;
using System.Windows.Media.Imaging;
using ViewFaceCore;
using ViewFaceCore.Configs;
using ViewFaceCore.Core;
using ViewFaceCore.Model;

namespace Shared.Helpers
{
    public static class FaceRecognition
    {
        private static VideoCapture videoCapture = new();
        private static readonly FaceDetector faceDetector;
        private static readonly FaceLandmarker faceMark;
        private static readonly FaceRecognizer faceRecognizer = new();
        private static readonly Pen pen = new(Color.Red, 2);
        private static readonly float videoCaptureFps = 18;

        public static bool IsCameraOpened { get; set; }

        public static int SleepTime
        {
            get
            {
                return (int)Math.Round(1000 / videoCaptureFps);
            }
        }

        public static Dictionary<string, int> SystemCameraDevices
        {
            get
            {
                using SystemDeviceEnumerator sde = new();
                Dictionary<string, int> devices = [];
                try
                {
                    IReadOnlyDictionary<int, string> systemDevices = sde.ListVideoInputDevice();
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
            FaceDetectConfig config = new()
            {
                FaceSize = 80
            };
            faceDetector = new(config);

            FaceLandmarkConfig faceLandmarkconfig = new()
            {
                MarkType = MarkType.Light,
                DeviceType = DeviceType.CPU
            };
            faceMark = new(faceLandmarkconfig);
        }

        public static Bitmap GetImage()
        {
            using Mat v_mat = new();
            videoCapture.Read(v_mat);
            if (v_mat != null)
            {
                return (Bitmap)Image.FromStream(v_mat.ToMemoryStream());
            }
            else
            {
                return new Bitmap(2, 2);
            }
        }

        public static Bitmap GetMaskImage(Bitmap cameraImage)
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

        public static (Bitmap, float[], int, int) GetMaskAndName(Bitmap cameraImage)
        {
            Bitmap mask = new(cameraImage.Width, cameraImage.Height);
            using Graphics maskGraphics = Graphics.FromImage(mask);

            FaceInfo[] faceInfos = faceDetector.Detect(cameraImage);
            if (faceInfos.Length > 0)
            {
                FaceInfo faceInfo = faceInfos[0];
                FaceMarkPoint[] faceMarkPoint = faceMark.Mark(cameraImage, faceInfo);

                maskGraphics.DrawRectangle(pen, faceInfo.Location.X, faceInfo.Location.Y, faceInfo.Location.Width, faceInfo.Location.Height);

                float[] feature = faceRecognizer.Extract(cameraImage, faceMarkPoint);
                return (mask, feature, faceInfo.Location.X, faceInfo.Location.Y);
            }
            else
            {
                return (mask, [], 0, 0);
            }

        }

        public async static ValueTask<EncodingFace> GetFace(Bitmap cameraImage)
        {
            FaceInfo[] faceInfos = await faceDetector.DetectAsync(cameraImage);

            if (faceInfos.Length > 0)
            {
                FaceInfo faceInfo = faceInfos[0];
                FaceMarkPoint[] faceMarkPoint = faceMark.Mark(cameraImage, faceInfo);

                Rectangle cropArea = new(faceInfo.Location.X, faceInfo.Location.Y, faceInfo.Location.Width, faceInfo.Location.Height);
                using Bitmap croppedBitmap = cameraImage.Clone(cropArea, cameraImage.PixelFormat);
                Bitmap a = ImageProcess.Resize(croppedBitmap, 164, 216);

                return new EncodingFace(cameraImage, ImageProcess.BitmapToBitmapImage(a), faceInfo, faceMarkPoint);
            }
            else
            {
                return new EncodingFace(new Bitmap(2, 2), new BitmapImage(), new FaceInfo(), []);
            }
        }

        public static List<float> GetFaceQualitys(List<EncodingFace> faces)
        {
            using FaceQuality faceQuality = new(new());
            List<float> faceQualitys = [];
            foreach (EncodingFace face in faces)
            {
                faceQualitys.Add(faceQuality.Detect(face.FullImage, face.FaceInfo, face.FaceMarkPoints, QualityType.Pose).Score);
            }
            return faceQualitys;
        }

        public static float[] GetFaceFeature(EncodingFace face)
        {
            return faceRecognizer.Extract(face.FullImage, face.FaceMarkPoints);
        }

        public static string GetFaceFeatureString(EncodingFace face)
        {
            return JsonConvert.SerializeObject(GetFaceFeature(face));
        }

        public static float[] GetFaceFeatureFromString(string faceFeature)
        {
            return JsonConvert.DeserializeObject<float[]>(faceFeature) ?? [];
        }

        public static bool IsSelf(float[] a, float[] b)
        {
            return faceRecognizer.IsSelf(a, b);
        }

        public static bool OpenCamera(int videoCapture_id, out string message)
        {
            videoCapture = new();
            videoCapture.Set(VideoCaptureProperties.FrameWidth, 960);
            videoCapture.Set(VideoCaptureProperties.FrameHeight, 564);//高度
            videoCapture.Set(VideoCaptureProperties.Fps, (int)videoCaptureFps);
            try
            {
                message = string.Empty;
                videoCapture.Open(videoCapture_id, VideoCaptureAPIs.DSHOW);
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
