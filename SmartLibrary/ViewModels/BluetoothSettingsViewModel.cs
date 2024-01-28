using SmartLibrary.Helpers;
using SmartLibrary.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SmartLibrary.ViewModels
{
    public partial class BluetoothSettingsViewModel : ObservableObject
    {
        private bool _isInitialized = false;
        private bool _isBleConnected = false;
        private int _connectedDeviceIndex = -1;

        private readonly BluetoothHelper ble = BluetoothHelper.Instance;

        [ObservableProperty]
        private string _stateText = string.Empty;

        [ObservableProperty]
        private string _statusImageSource = string.Empty;

        [ObservableProperty]
        private string _scanButtonText = string.Empty;

        [ObservableProperty]
        private bool _scanButtonEnabled = true;

        [ObservableProperty]
        private bool _progressBarIsIndeterminate = false;

        [ObservableProperty]
        private bool _progressBarVisibility = false;

        [ObservableProperty]
        private string _connectButtonText = "连接";

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
            if(!_isInitialized)
            {
                OnRefleshButtonClick();
                ble.DiscoverDevice += DiscoverDevice;
                ble.DiscoverComplete += ScanComplete;

                ble.ConnectEvent += ConnectEvent;

                _isInitialized = true;
            }
        }

        [RelayCommand]
        private void OnRefleshButtonClick()
        {
            ble.RefreshState();
            if (!ble.IsPlatformSupportBT())
            {
                StateText = "蓝牙未启用";
                ScanButtonText = "开启蓝牙";
                StatusImageSource = "pack://application:,,,/Assets/bluetooth-disabled.png";
            }
            else
            {
                StateText = "蓝牙未连接";
                ScanButtonText = "扫描设备";
                StatusImageSource = "pack://application:,,,/Assets/bluetooth.png";
            }
        }

        [RelayCommand]
        private void OnScanButtonClick()
        {
            if(ble.IsPlatformSupportBT())
            {
                ListViewItems.Clear();
                ScanButtonEnabled = ConnectButtonEnabled = false;
                ScanButtonText = "正在扫描";
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
            ScanButtonEnabled = true;
            ScanButtonText = "扫描设备";
            ProgressBarIsIndeterminate = ProgressBarVisibility = false;
        }

        public void OnListViewSelecteChanged()
        {
            if(ListviewSelectedIndex == -1)
            {
                ConnectButtonEnabled = false; 
            }
            else
            {
                ConnectButtonEnabled = true;
            }
        }

        [RelayCommand]
        private void OnConnectButtonClick()
        {
            if (!_isBleConnected)
            {
                StateText = "正在连接 " + ListViewItems[ListviewSelectedIndex].Name;
                ScanButtonEnabled = ConnectButtonEnabled = false;
                ListviewEnabled = false;
                ConnectButtonEnabled = ProgressBarIsIndeterminate = true;
                ble.StartConnect(ListViewItems[ListviewSelectedIndex].Address);
            }
            else
            {

            }
        }

        private void ConnectEvent(string info)
        {
            if(info == "Success")
            {
                _isBleConnected = true;
                _connectedDeviceIndex = ListviewSelectedIndex;
                StateText = ListViewItems[ListviewSelectedIndex].Name + " 已连接";
            }
            else
            {
                StateText = "连接失败-" + info;
            }
            ScanButtonEnabled = ConnectButtonEnabled = true;
            ListviewEnabled = true;
            ConnectButtonEnabled = ProgressBarIsIndeterminate = false;
        }
    }
}
