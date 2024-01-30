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

        public void StartScan()
        {
            BluetoothComponent bluetoothComponent = new BluetoothComponent(bluetoothClient);
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
            _BGconnectWorker.RunWorkerAsync(address);
        }

        private void BGConnectWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            string? address = e.Argument as string;
            if (address != null)
            {
                BluetoothAddress btAddress = BluetoothAddress.Parse(address);
                BluetoothDeviceInfo bluetoothDevice = new BluetoothDeviceInfo(btAddress);
                // 检测是否配对
                if (!bluetoothDevice.Authenticated)
                {
                    System.Timers.Timer tmr = new System.Timers.Timer(2500);
                    tmr.AutoReset = false;
                    tmr.Elapsed += OnPairing;
                    tmr.Start();
                }
                // "00001124-0000-1000-8000-00805f9b34fb"
                bluetoothClient?.Connect(btAddress, BluetoothService.Handsfree);
            }
        }

        private void OnPairing(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ConnectEvent("正在配对");
        }

        private void BGConnectWorker_Completed(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ConnectEvent("连接成功");
            }
            else
            {
                string allMessage = e.Error.Message;
                string[] sArray = allMessage.Split("。");
                ConnectEvent(sArray[0]);
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