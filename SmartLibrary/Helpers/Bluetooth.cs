using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.ComponentModel;

namespace SmartLibrary.Helpers
{
    public class BluetoothHelper
    {
        private static BluetoothHelper? _instance;
        private BluetoothClient? bluetoothClient;
        private BluetoothRadio bluetoothRadio = BluetoothRadio.PrimaryRadio;

        //连接线程设置
        private readonly BackgroundWorker _BGconnectWorker = new BackgroundWorker();

        public delegate void DiscoverDeviceEventHandler(List<string> deviceInfo);
        public delegate void ConnectEventHandler(string info);
        public event DiscoverDeviceEventHandler DiscoverDevice = delegate { };
        public event EventHandler DiscoverComplete = delegate { };
        public event ConnectEventHandler ConnectEvent = delegate { };

        public static BluetoothHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BluetoothHelper();
                }
                return _instance;
            }
        }

        private BluetoothHelper()
        {
            if (IsPlatformSupportBT())
            {
                bluetoothClient = new BluetoothClient();
                bluetoothRadio.Mode = RadioMode.Connectable;
            }
            _BGconnectWorker.DoWork += BGConnectWorker_DoWork;
            _BGconnectWorker.RunWorkerCompleted += BGConnectWorker_Completed;
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
                    bluetoothRadio.Mode = RadioMode.Discoverable;
                }
            }
        }

        public void StartScan()
        {
            BluetoothComponent bluetoothComponent = new BluetoothComponent(bluetoothClient);
            bluetoothComponent.DiscoverDevicesAsync(255, true, true, true, true, bluetoothComponent);
            bluetoothComponent.DiscoverDevicesProgress += BluetoothComponent_DiscoverDevice;
            bluetoothComponent.DiscoverDevicesComplete += BluetoothComponent_DiscoverComplete;
        }

        private void BluetoothComponent_DiscoverDevice(object? sender, DiscoverDevicesEventArgs e)
        {
            List<string> deviceInfo =
            [
                e.Devices[0].DeviceName,
                e.Devices[0].DeviceAddress.ToString("C"),
                e.Devices[0].ClassOfDevice.MajorDevice.ToString(),
            ];
            DiscoverDevice(deviceInfo);
        }

        private void BluetoothComponent_DiscoverComplete(object? sender, DiscoverDevicesEventArgs e)
        {
            DiscoverComplete(sender,e);
        }

        public void StartConnect(string address)
        {
            _BGconnectWorker.RunWorkerAsync(address);
        }

        private void BGConnectWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            string? address = e.Argument as string;
            if (address != null)
            {
                BluetoothAddress btAddress = BluetoothAddress.Parse(address);
                // "00001124-0000-1000-8000-00805f9b34fb"
                BluetoothEndPoint ep = new BluetoothEndPoint(btAddress, BluetoothService.SerialPort);
                bluetoothClient?.Connect(ep);
            }
        }

        private void BGConnectWorker_Completed(object? sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                ConnectEvent("Success");
            }
            else
            {
                ConnectEvent(e.Error.Message);
            }
        }

        public void StartDisconnect()
        {
            if(bluetoothClient != null)
            {
                bluetoothClient.Close();
                bluetoothClient = new BluetoothClient();
            }
        }
    }
}
