using Hompus.VideoInputDevices;

namespace Shared.Helpers
{
    public class FaceRecognition
    {
        public static Dictionary<string, int> GetSystemCameraDevices()
        {
            using SystemDeviceEnumerator sde = new();
            Dictionary<string, int> devices = [];
            foreach (var device in sde.ListVideoInputDevice())
            {
                devices.Add(device.Value, device.Key);
            }
            return devices;
        }
    }
}
