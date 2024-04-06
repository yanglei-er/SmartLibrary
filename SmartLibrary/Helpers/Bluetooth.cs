using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using SmartLibrary.Models;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;

namespace SmartLibrary.Helpers
{
    public static class BluetoothHelper
    {
        private static BluetoothClient? bluetoothClient;
        private static BluetoothRadio? bluetoothRadio;

        private static System.Timers.Timer ListenerTimer;

        public delegate void ConnectEventHandler(string info);

        public static event ConnectEventHandler ConnectEvent = delegate { };

        public delegate void DiscoverDeviceEventHandler(BluetoothDevice deviceInfo);

        public static event DiscoverDeviceEventHandler DiscoverDevice = delegate { };

        public delegate void DiscoverCompleteEventHandler(string info);

        public static event DiscoverCompleteEventHandler DiscoverComplete = delegate { };

        public delegate void ReceiveEventHandler(string message);

        public static event ReceiveEventHandler ReceiveEvent = delegate { };

        static BluetoothHelper()
        {
            _previousBleState = IsPlatformSupportBT();
            ListenerTimer = new(200)
            {
                Enabled = false,
                AutoReset = true
            };
            ListenerTimer.Elapsed += Listener;
        }

        #region 蓝牙状态改变
        public delegate void BleStateChangedEventHandler(bool state);

        public static event BleStateChangedEventHandler BleStateChangedEvent = delegate { };

        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int WM_DEVICECHANGE = 0x0219;
        private static bool _previousBleState;

        public static IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
        #endregion 蓝牙状态改变

        public static bool IsPlatformSupportBT()
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

        public static bool IsBleConnected
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

        public static void StartScan()
        {
            BluetoothComponent bluetoothComponent = new(bluetoothClient);
            bluetoothComponent.DiscoverDevicesAsync(255, false, false, true, false, bluetoothComponent);
            bluetoothComponent.DiscoverDevicesProgress += BluetoothComponent_DiscoverDevice;
            bluetoothComponent.DiscoverDevicesComplete += BluetoothComponent_DiscoverComplete;
        }

        private static void BluetoothComponent_DiscoverDevice(object? sender, DiscoverDevicesEventArgs e)
        {
            BluetoothDevice deviceInfo = new(e.Devices[0].DeviceName, e.Devices[0].DeviceAddress.ToString("C"), e.Devices[0].ClassOfDevice.MajorDevice.ToString(), e.Devices[0].Authenticated
                       );
            DiscoverDevice(deviceInfo);
        }

        private static void BluetoothComponent_DiscoverComplete(object? sender, DiscoverDevicesEventArgs e)
        {
            DiscoverComplete("完成");
        }

        public static void StartConnect(BluetoothDevice device)
        {
            Task.Run(() => ConnectAction(device));
        }

        private static void ConnectAction(BluetoothDevice device)
        {
            BluetoothAddress btAddress = BluetoothAddress.Parse(device.Address);

            using System.Timers.Timer tmr = new(2500)
            {
                AutoReset = false
            };
            tmr.Elapsed += (_, _) => ConnectEvent("正在配对");

            // 检测是否配对
            if (!device.IsAuthenticated)
            {
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
                tmr.Stop();
                ConnectEvent(ex.Message);
            }
        }

        public static void StartDisconnect()
        {
            if (bluetoothClient != null)
            {
                if (bluetoothClient.Connected)
                {
                    bluetoothClient.Close();
                    bluetoothClient = new BluetoothClient();
                }
            }
        }

        public static async void Send(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (bluetoothClient != null)
                {
                    Stream bluetoothStream = bluetoothClient.GetStream();
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await bluetoothStream.WriteAsync(buffer);
                    await bluetoothStream.FlushAsync();
                    bluetoothStream.Close();

                    ListenerTimer.Start();
                }
            }
        }

        private static async void Listener(object? obj, ElapsedEventArgs args)
        {
            if (bluetoothClient != null)
            {
                try
                {
                    Stream bluetoothStream = bluetoothClient.GetStream();
                    byte[] buffer = [255];
                    await Task.Delay(300);
                    await bluetoothStream.ReadAsync(buffer);
                    string message = Encoding.UTF8.GetString(buffer).Replace("\0", "");
                    ReceiveEvent(message);
                    ListenerTimer.Stop();
                }
                catch
                {

                }
            }
        }
    }
}