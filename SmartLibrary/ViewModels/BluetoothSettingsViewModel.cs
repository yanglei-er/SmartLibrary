namespace SmartLibrary.ViewModels
{
    public partial class BluetoothSettingsViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _title = string.Empty;

        public BluetoothSettingsViewModel() 
        { 
            if(!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        private void InitializeViewModel() 
        {
            Title = "蓝牙未连接";
            _isInitialized = true;
        }
    }
}
