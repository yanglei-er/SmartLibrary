using SmartLibrary.Helpers;

namespace SmartLibrary.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _autoStart = Convert.ToBoolean(SettingsHelper.GetConfig("AutoStart"));

        [ObservableProperty]
        private bool _autoStartMinimized = Convert.ToBoolean(SettingsHelper.GetConfig("AutoStartMinimized"));

        [ObservableProperty]
        private bool _trayEnabled = Convert.ToBoolean(SettingsHelper.GetConfig("TrayEnabled"));

        [ObservableProperty]
        private bool _autoCheckUpdate = Convert.ToBoolean(SettingsHelper.GetConfig("AutoCheckUpdate"));

        [ObservableProperty]
        private bool _hardwareRendering = Convert.ToBoolean(SettingsHelper.GetConfig("HardwareRendering"));

        [ObservableProperty]
        private int _currentApplicationThemeIndex = GetCurrentApplicationThemeIndex(SettingsHelper.GetConfig("Theme"));

        partial void OnAutoStartChanged(bool value)
        {
            SettingsHelper.SetConfig("AutoStart", value.ToString());
        }

        partial void OnAutoStartMinimizedChanged(bool value)
        {
            SettingsHelper.SetConfig("AutoStartMinimized", value.ToString());
        }

        partial void OnTrayEnabledChanged(bool value)
        {
            SettingsHelper.SetConfig("TrayEnabled", value.ToString());
        }

        partial void OnAutoCheckUpdateChanged(bool value)
        {
            SettingsHelper.SetConfig("AutoCheckUpdate", value.ToString());
        }

        partial void OnHardwareRenderingChanged(bool value)
        {
            SettingsHelper.SetConfig("hardwareRendering", value.ToString());
        }

        partial void OnCurrentApplicationThemeIndexChanged(int value)
        {
            if (value == 0)
            {
                SettingsHelper.SetConfig("Theme", "System");
            }
            else if (value == 1)
            {
                SettingsHelper.SetConfig("Theme", "Light");
            }
            else
            {
                SettingsHelper.SetConfig("Theme", "Dark");
            }
        }

        private static int GetCurrentApplicationThemeIndex(string theme)
        {
            if (theme == "System")
            {
                return 0;
            }
            else if (theme == "Light")
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }
}