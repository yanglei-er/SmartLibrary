using SmartLibrary.Helpers;
using SmartLibrary.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SmartLibrary.ViewModels
{
    public partial class BluetoothSettingsViewModel : ObservableObject
    {
        private string _connectedName = string.Empty;

        private readonly BluetoothHelper ble = BluetoothHelper.Instance;

        [ObservableProperty]
        private string _stateText = string.Empty;

        [ObservableProperty]
        private string _stateImageSource = string.Empty;

        [ObservableProperty]
        private string _scanButtonText = string.Empty;

        [ObservableProperty]
        private bool _scanButtonEnabled = true;

        [ObservableProperty]
        private bool _progressBarIsIndeterminate = false;

        [ObservableProperty]
        private bool _progressBarVisibility = false;

        [ObservableProperty]
        private string _connectButtonText = "连接设备";

        [ObservableProperty]
        private bool _connectButtonEnabled = false;

        [ObservableProperty]
        private bool _listviewEnabled = true;

        [ObservableProperty]
        private int _listviewSelectedIndex = -1;

        [ObservableProperty]
        private ObservableCollection<BluetoothDevice> _listViewItems = new ObservableCollection<BluetoothDevice>();

        public BluetoothSettingsViewModel()
        {
            OnRefleshButtonClick();
            ble.DiscoverDevice += DiscoverDevice;
            ble.DiscoverComplete += ScanComplete;
            ble.ConnectEvent += ConnectEvent;
            ble.BleStateChangedEvent += OnBleStateChanged;
        }

        ~BluetoothSettingsViewModel()
        {
            ble.DiscoverDevice -= DiscoverDevice;
            ble.DiscoverComplete -= ScanComplete;
            ble.ConnectEvent -= ConnectEvent;
            ble.BleStateChangedEvent -= OnBleStateChanged;
        }

        [RelayCommand]
        private void OnRefleshButtonClick()
        {
            if (!ble.IsPlatformSupportBT())
            {
                StateText = "蓝牙未启用";
                StateImageSource = "pack://application:,,,/Assets/bluetooth-disabled.png";
                ScanButtonText = "开启蓝牙";
                ConnectButtonEnabled = false;
                ListViewItems.Clear();
            }
            else
            {
                if (!ble.isBleConnected())
                {
                    StateText = "蓝牙未连接";
                    ScanButtonText = "扫描设备";
                    StateImageSource = "pack://application:,,,/Assets/bluetooth.png";
                }
            }
        }

        private void OnBleStateChanged(bool state)
        {
            if (state)
            {
                StateText = "蓝牙未连接";
                ScanButtonText = "扫描设备";
                StateImageSource = "pack://application:,,,/Assets/bluetooth.png";
            }
            else
            {
                StateText = "蓝牙未启用";
                ScanButtonText = "开启蓝牙";
                StateImageSource = "pack://application:,,,/Assets/bluetooth-disabled.png";
                ConnectButtonEnabled = false;
                ListViewItems.Clear();
            }
        }

        [RelayCommand]
        private void OnScanButtonClick()
        {
            if (ble.IsPlatformSupportBT())
            {
                StateText = "正在扫描设备";
                ListViewItems.Clear();
                ScanButtonEnabled = ConnectButtonEnabled = false;
                ProgressBarVisibility = ProgressBarIsIndeterminate = true;
                ble.StartScan();
            }
            else
            {
                var process = new Process { StartInfo = { FileName = "control", Arguments = "bthprops.cpl" } };
                process.Start();
            }
        }

        private void DiscoverDevice(List<string> deviceInfo)
        {
            ListViewItems.Add(new BluetoothDevice(deviceInfo[0], deviceInfo[1], deviceInfo[2]));
        }

        private void ScanComplete(object? sender, EventArgs e)
        {
            StateText = "一共扫描到 " + ListViewItems.Count.ToString() + " 个设备";
            ScanButtonEnabled = true;
            ProgressBarIsIndeterminate = ProgressBarVisibility = false;
        }

        partial void OnListviewSelectedIndexChanged(int value)
        {
            if (ListviewSelectedIndex != -1)
            {
                ConnectButtonEnabled = true;
                if (ble.isBleConnected())
                {
                    if (ListViewItems[ListviewSelectedIndex].Name == _connectedName)
                    {
                        ConnectButtonText = "断开连接";
                    }
                    else
                    {
                        ConnectButtonText = "连接新设备";
                    }
                }
            }
        }

        [RelayCommand]
        private void OnConnectButtonClick()
        {
            if (!ble.isBleConnected())
            {
                StateText = "正在连接 " + ListViewItems[ListviewSelectedIndex].Name;
                ScanButtonEnabled = ConnectButtonEnabled = false;
                ListviewEnabled = false;
                ProgressBarVisibility = ProgressBarIsIndeterminate = true;
                ble.StartConnect(ListViewItems[ListviewSelectedIndex].Address);
            }
            else
            {
                ble.StartDisconnect();
                if (ConnectButtonText == "连接新设备")
                {
                    OnConnectButtonClick();
                }
                else
                {
                    StateImageSource = "pack://application:,,,/Assets/bluetooth.png";
                    StateText = "蓝牙未连接";
                    ConnectButtonText = "连接设备";
                    _connectedName = string.Empty;
                }
            }
        }

        private void ConnectEvent(string info)
        {
            if (info == "正在配对")
            {
                StateText = "正在与 " + ListViewItems[ListviewSelectedIndex].Name + " 配对";
                return;
            }
            else if (info == "连接成功")
            {
                StateText = ListViewItems[ListviewSelectedIndex].Name + " 已连接";
                StateImageSource = "pack://application:,,,/Assets/bluetooth-connected.png";
                ConnectButtonText = "断开连接";
                _connectedName = ListViewItems[ListviewSelectedIndex].Name;
            }
            else
            {
                StateText = "连接失败-" + info;
            }
            ScanButtonEnabled = ConnectButtonEnabled = true;
            ListviewEnabled = true;
            ProgressBarVisibility = ProgressBarIsIndeterminate = false;
        }
    }
}
