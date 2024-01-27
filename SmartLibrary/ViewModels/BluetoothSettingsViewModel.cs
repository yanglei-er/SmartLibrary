using SmartLibrary.Helpers;
using SmartLibrary.Models;
using System.Collections.ObjectModel;

namespace SmartLibrary.ViewModels
{
    public partial class BluetoothSettingsViewModel : ObservableObject
    {
        private bool _isInitialized = false;
        private readonly Bluetooth ble = new Bluetooth();

        [ObservableProperty]
        private string _stateText = string.Empty;

        [ObservableProperty]
        private string _scanButtonText = "扫描设备";

        [ObservableProperty]
        private bool _scanButtonEnabled = false;

        [ObservableProperty]
        private bool _refelshButtonDisabled = true;

        [ObservableProperty]
        private bool _progressBarIsIndeterminate = false;

        [ObservableProperty]
        private bool _progressBarVisibility = false;

        [ObservableProperty]
        private string _statusImageSource = "pack://application:,,,/Assets/bluetooth.png";

        [ObservableProperty]
        private ObservableCollection<BluetoothDevice> _listViewItems = new ObservableCollection<BluetoothDevice>();

        [RelayCommand]
        private void OnRefleshButtonClick()
        {
            ble.RefreshState();
            if (!ble.IsPlatformSupportBT())
            {
                StateText = "蓝牙未启用";
                ScanButtonEnabled = false;
            }
            else
            {
                StateText = "蓝牙未连接";
                ScanButtonEnabled = true;
            }
        }

        [RelayCommand]
        private void OnScanButtonClick()
        {
            ListViewItems.Clear();
            ScanButtonEnabled = false;
            RefelshButtonDisabled = false;
            ScanButtonText = "正在扫描";
            ProgressBarIsIndeterminate = true;
            ProgressBarVisibility = true;
            ble.StartScan();
        }

        public BluetoothSettingsViewModel() 
        { 
            if(!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        private void InitializeViewModel() 
        {
            if (!ble.IsPlatformSupportBT())
            {
                StateText = "蓝牙未启用";
                ScanButtonEnabled = false;
            }
            else
            {
                StateText = "蓝牙未连接";
                ScanButtonEnabled = true;
            }

            ble.DiscoverDevice += DiscoverDevice;
            ble.DiscoverComplete += ScanComplete;

            _isInitialized = true;
        }

        private void DiscoverDevice(List<string> deviceInfo)
        {
            ListViewItems.Add(new BluetoothDevice(deviceInfo[0], deviceInfo[1], deviceInfo[2]));
        }

        private void ScanComplete(object? sender, EventArgs e)
        {
            ScanButtonEnabled = true;
            RefelshButtonDisabled = true;
            ScanButtonText = "扫描设备";
            ProgressBarIsIndeterminate = false;
            ProgressBarVisibility = false;
        }
    }
}
