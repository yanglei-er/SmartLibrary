using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace SmartLibrary.Helpers
{
    public class Bluetooth
    {
        private BluetoothClient? bluetoothClient;
        private BluetoothRadio bluetoothRadio = BluetoothRadio.PrimaryRadio;

        public delegate void DiscoverDeviceEventHandl(List<string> deviceInfo);
        public event DiscoverDeviceEventHandl DiscoverDevice = delegate { };
        public event EventHandler DiscoverComplete = delegate { };

        public Bluetooth()
        {
            if (IsPlatformSupportBT())
            {
                bluetoothClient = new BluetoothClient();
                bluetoothRadio.Mode = RadioMode.Connectable;
            }
        }

        public bool IsPlatformSupportBT()
        {
            if (bluetoothRadio == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void RefreshState()
        {
            bluetoothRadio = BluetoothRadio.PrimaryRadio;
            if (IsPlatformSupportBT())
            {
                if(bluetoothClient == null)
                {
                    bluetoothClient = new BluetoothClient();
                    bluetoothRadio.Mode = RadioMode.Connectable;
                }
            }
        }

        public void StartScan()
        {
            BluetoothComponent bluetoothComponent = new BluetoothComponent(bluetoothClient);
            bluetoothComponent.DiscoverDevicesAsync(10, true, true, true, true, bluetoothComponent);
            bluetoothComponent.DiscoverDevicesProgress += BluetoothComponent_DiscoverDevice;
            bluetoothComponent.DiscoverDevicesComplete += BluetoothComponent_DiscoverComplete;
        }

        private void BluetoothComponent_DiscoverDevice(object? sender, DiscoverDevicesEventArgs e)
        {
            
            List<string> deviceInfo =
            [
                e.Devices[0].DeviceName.ToString(),
                "地址："+e.Devices[0].DeviceAddress.ToString(),
                "设备类型："+e.Devices[0].ClassOfDevice.MajorDevice.ToString(),
            ];
            DiscoverDevice(deviceInfo);
        }

        private void BluetoothComponent_DiscoverComplete(object? sender, DiscoverDevicesEventArgs e)
        {
            DiscoverComplete(sender, e);
        }
    }
}
