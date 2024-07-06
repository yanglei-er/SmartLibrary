using Hompus.VideoInputDevices;

namespace Shared.Helpers
{
    public class FaceRecognition
    {
        public static Dictionary<string, int> GetSystemCameraDevices()
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
}
