using Hompus.VideoInputDevices;
using Newtonsoft.Json;
using OpenCvSharp;
using Shared.Models;
using System.Drawing;
using System.Windows;
using ViewFaceCore;
using ViewFaceCore.Configs;
using ViewFaceCore.Core;
using ViewFaceCore.Model;

namespace Shared.Helpers
{
    public static class FaceRecognition
    {
        private static VideoCapture videoCapture = new();
        private static readonly FaceDetector faceDetector = new();
        private static readonly FaceLandmarker faceMark;
        private static readonly FaceRecognizer faceRecognizer = new();
        private static readonly FaceQuality faceQuality;
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
            FaceLandmarkConfig faceLandmarkconfig = new()
            {
                MarkType = MarkType.Light,
                DeviceType = DeviceType.CPU
            };
            faceMark = new(faceLandmarkconfig);

            QualityConfig qualityConfig = new();
            faceQuality = new(qualityConfig);
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
                return new Bitmap(1, 1);
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

        public async static ValueTask<Face> GetFace(Bitmap cameraImage)
        {
            FaceInfo[] faceInfos = await faceDetector.DetectAsync(cameraImage);

            if (faceInfos.Length > 0)
            {
                FaceInfo faceInfo = faceInfos[0];

                Rectangle cropArea = new(faceInfo.Location.X, faceInfo.Location.Y, faceInfo.Location.Width, faceInfo.Location.Height);
                using Bitmap croppedBitmap = cameraImage.Clone(cropArea, cameraImage.PixelFormat);
                Bitmap a = ImageProcess.Resize(croppedBitmap, 164, 216);

                FaceMarkPoint[] faceMarkPoint = faceMark.Mark(a, faceDetector.Detect(a)[0]);
                return new Face(a, faceInfo, faceMarkPoint);
            }
            else
            {
                Bitmap bitmap = new(2, 2);
                return new Face(bitmap, new FaceInfo(), []);
            }
        }

        public static float[] GetFaceFeature(Face face)
        {
            return faceRecognizer.Extract(face.FaceImage, face.FaceMarkPoints);
        }

        public static float GetFaceQuality(Face face)
        {
            QualityResult result = faceQuality.Detect(face.FaceImage, face.FaceInfo, face.FaceMarkPoints, QualityType.Pose);
            return result.Score;
        }

        public static string GetFaceFeatureString(Face face)
        {
           return JsonConvert.SerializeObject(GetFaceFeature(face));
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
