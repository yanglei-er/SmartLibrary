using SmartLibrary.Helpers;
using SmartLibrary.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SmartLibrary.ViewModels
{
    public partial class BluetoothSettingsViewModel : ObservableObject
    {
        private string _connectedName = string.Empty;

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
        private ObservableCollection<BluetoothDevice> _listViewItems = [];

        public BluetoothSettingsViewModel()
        {
            OnRefleshButtonClick();
            BluetoothHelper.DiscoverDevice += DiscoverDevice;
            BluetoothHelper.DiscoverComplete += ScanComplete;
            BluetoothHelper.ConnectEvent += ConnectEvent;
            BluetoothHelper.BleStateChangedEvent += OnBleStateChanged;
        }

        ~BluetoothSettingsViewModel()
        {
            BluetoothHelper.DiscoverDevice -= DiscoverDevice;
            BluetoothHelper.DiscoverComplete -= ScanComplete;
            BluetoothHelper.ConnectEvent -= ConnectEvent;
            BluetoothHelper.BleStateChangedEvent -= OnBleStateChanged;
        }

        [RelayCommand]
        private void OnRefleshButtonClick()
        {
            if (!BluetoothHelper.IsPlatformSupportBT())
            {
                StateText = "蓝牙未启用";
                StateImageSource = "pack://application:,,,/Assets/bluetooth-disabled.png";
                ScanButtonText = "开启蓝牙";
                ConnectButtonEnabled = false;
                ListViewItems.Clear();
            }
            else
            {
                if (!BluetoothHelper.IsBleConnected)
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
            if (BluetoothHelper.IsPlatformSupportBT())
            {
                StateText = "正在扫描设备";
                ListViewItems.Clear();
                ScanButtonEnabled = ConnectButtonEnabled = false;
                ProgressBarVisibility = ProgressBarIsIndeterminate = true;
                BluetoothHelper.StartScan();
            }
            else
            {
                Process process = new();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.CreateNoWindow = true;//不显示程序窗口
                process.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                process.Start();//启动程序
                process.StandardInput.WriteLine("start ms-settings:bluetooth &exit");
                process.StandardInput.AutoFlush = true;
                process.WaitForExit();
                process.Close();
            }
        }

        private void DiscoverDevice(BluetoothDevice device)
        {
            ListViewItems.Add(device);
        }

        private void ScanComplete(string info)
        {
            if (info == "完成")
            {
                StateText = "一共扫描到 " + ListViewItems.Count.ToString() + " 个设备";
            }
            else
            {
                StateText = info;
            }
            ScanButtonText = "扫描设备";
            ScanButtonEnabled = true;
            ProgressBarIsIndeterminate = ProgressBarVisibility = false;
        }

        partial void OnListviewSelectedIndexChanged(int value)
        {
            if (ListviewSelectedIndex != -1)
            {
                ConnectButtonEnabled = true;
                if (BluetoothHelper.IsBleConnected)
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
            if (BluetoothHelper.IsBleConnected)
            {
                BluetoothHelper.StartDisconnect();
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
            else
            {
                StateText = "正在连接 " + ListViewItems[ListviewSelectedIndex].Name;
                ScanButtonEnabled = ConnectButtonEnabled = false;
                ListviewEnabled = false;
                ProgressBarVisibility = ProgressBarIsIndeterminate = true;
                BluetoothHelper.StartConnect(ListViewItems[ListviewSelectedIndex]);
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