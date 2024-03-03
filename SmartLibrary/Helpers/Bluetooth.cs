using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Net.NetworkInformation;

namespace SmartLibrary.Helpers
{
    public sealed class BluetoothHelper
    {
        private static BluetoothHelper? _instance;
        private BluetoothClient? bluetoothClient;
        private BluetoothRadio? bluetoothRadio;

        public delegate void ConnectEventHandler(string info);

        public event ConnectEventHandler ConnectEvent = delegate { };

        public delegate void DiscoverDeviceEventHandler(List<string> deviceInfo);

        public event DiscoverDeviceEventHandler DiscoverDevice = delegate { };

        public event EventHandler DiscoverComplete = delegate { };

        public static BluetoothHelper Instance
        {
            get
            {
                _instance ??= new BluetoothHelper();
                return _instance;
            }
        }

        private BluetoothHelper()
        {
            _previousBleState = IsPlatformSupportBT();
        }

        //蓝牙状态改变
        public delegate void BleStateChangedEventHandler(bool state);

        public event BleStateChangedEventHandler BleStateChangedEvent = delegate { };

        public const int DBT_DEVNODES_CHANGED = 0x0007;
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
                        bluetoothRadio = BluetoothRadio.PrimaryRadio;
                        bluetoothRadio.Mode = RadioMode.Connectable;
                        bluetoothRadio.Mode = RadioMode.Discoverable;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool IsBleConnected
        {
            get
            {
                if (bluetoothClient != null)
                {
                    return bluetoothClient.Connected;
                }
                return false;
            }
        }

        public void StartScan()
        {
            BluetoothComponent bluetoothComponent = new(bluetoothClient);
            bluetoothComponent.DiscoverDevicesAsync(255, false, false, true, false, bluetoothComponent);
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
                e.Devices[0].Connected.ToString(),
            ];
            DiscoverDevice(deviceInfo);
        }

        private void BluetoothComponent_DiscoverComplete(object? sender, DiscoverDevicesEventArgs e)
        {
            DiscoverComplete(sender, e);
        }

        public void StartConnect(string address)
        {
            Task.Run(() => ConnectAction(address));
        }

        private void ConnectAction(string address)
        {
            BluetoothAddress btAddress = BluetoothAddress.Parse(address);
            BluetoothDeviceInfo bluetoothDevice = new(btAddress);
            // 检测是否配对
            if (!bluetoothDevice.Authenticated)
            {
                System.Timers.Timer tmr = new(2500)
                {
                    AutoReset = false
                };
                tmr.Elapsed += OnPairing;
                tmr.Start();
            }
            try
            {
                // "00001124-0000-1000-8000-00805f9b34fb"
                bluetoothClient?.Connect(btAddress, BluetoothService.Handsfree);
                ConnectEvent("连接成功");
            }
            catch (Exception ex)
            {
                ConnectEvent(ex.Message);
            }
        }

        private void OnPairing(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ConnectEvent("正在配对");
        }

        public void StartDisconnect()
        {
            if (bluetoothClient != null)
            {
                bluetoothClient.Close();
                bluetoothClient = new BluetoothClient();
            }
        }
    }
}