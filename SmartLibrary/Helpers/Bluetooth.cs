using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace SmartLibrary.Helpers
{
    public sealed class BluetoothHelper
    {
        private static BluetoothHelper? _instance;
        private BluetoothClient? bluetoothClient;
        private BluetoothRadio? bluetoothRadio;

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
            _previousBleState = IsPlatformSupportBT();
            _BGconnectWorker.DoWork += BGConnectWorker_DoWork;
            _BGconnectWorker.RunWorkerCompleted += BGConnectWorker_Completed;
        }

        ~BluetoothHelper()
        {
            _BGconnectWorker.DoWork -= BGConnectWorker_DoWork;
            _BGconnectWorker.RunWorkerCompleted -= BGConnectWorker_Completed;
        }

        //蓝牙状态改变
        public delegate void BleStateChangedEventHandler(bool state);
        public event BleStateChangedEventHandler BleStateChangedEvent = delegate { };
        public const int DBT_DEVNODES_CHANGED = 0x0007; //A device has been added to or removed from the system.
        private const int WM_DEVICECHANGE = 0x0219;
        public bool _previousBleState;
        public IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE)
            {
                if (wParam.ToInt32() == DBT_DEVNODES_CHANGED)
                {
                    bool _newBleState = IsPlatformSupportBT();
                    if (_newBleState != _previousBleState)
                    {
                        BleStateChangedEvent(_newBleState);
                        _previousBleState = _newBleState;
                    }
                }
            }
            return IntPtr.Zero;
        }

        public bool IsPlatformSupportBT()
        {
            NetworkInterface[] network = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in network)
            {
                if (networkInterface.Name.Contains("蓝牙"))
                {
                    if (bluetoothClient == null)
                    {
                        bluetoothClient = new BluetoothClient();
                        bluetoothRadio = BluetoothRadio.Default;
                        bluetoothRadio.Mode = RadioMode.Connectable;
                        bluetoothRadio.Mode = RadioMode.Discoverable;
                    }
                    return true;
                }
            }
            return false;
        }

        public void StartScan()
        {
            //BluetoothComponent bluetoothComponent = new BluetoothComponent(bluetoothClient);
            //bluetoothComponent.DiscoverDevicesAsync(255, true, true, true, true, bluetoothComponent);
            //bluetoothComponent.DiscoverDevicesProgress += BluetoothComponent_DiscoverDevice;
            //bluetoothComponent.DiscoverDevicesComplete += BluetoothComponent_DiscoverComplete;
        }

        private void BluetoothComponent_DiscoverDevice(object? sender, EventArgs e)
        {
            //List<string> deviceInfo =
            //[
            //    e.Devices[0].DeviceName,
            //    e.Devices[0].DeviceAddress.ToString("C"),
            //    e.Devices[0].ClassOfDevice.MajorDevice.ToString(),
            //];
            //DiscoverDevice(deviceInfo);
        }

        private void BluetoothComponent_DiscoverComplete(object? sender, EventArgs e)
        {
            DiscoverComplete(sender, e);
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
            if (e.Error == null)
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
            if (bluetoothClient != null)
            {
                bluetoothClient.Close();
                bluetoothClient = new BluetoothClient();
            }
        }

        public bool isBleConnected()
        {
            if (bluetoothClient != null)
            {
                return bluetoothClient.Connected;
            }
            return false;
        }
    }
}