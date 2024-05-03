using SmartLibrary.Helpers;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartLibrary.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private bool _autoStart = Convert.ToBoolean(SettingsHelper.GetConfig("AutoStart"));

        [ObservableProperty]
        private bool _autoStartMinimized = Convert.ToBoolean(SettingsHelper.GetConfig("AutoStartMinimized"));

        [ObservableProperty]
        private bool _trayEnabled = Convert.ToBoolean(SettingsHelper.GetConfig("TrayEnabled"));

        [ObservableProperty]
        private bool _autoCheckUpdate = Convert.ToBoolean(SettingsHelper.GetConfig("AutoCheckUpdate"));

        #region FileOccupancy
        [ObservableProperty]
        private bool _isFileOccupancyExpanded = false;
        [ObservableProperty]
        private string _dataCount = "正在计算";
        [ObservableProperty]
        private string _pictureCacheCount = "正在计算";
        [ObservableProperty]
        private string _tempCount = "正在计算";
        #endregion FileOccupancy

        [ObservableProperty]
        private int _currentApplicationThemeIndex = Helpers.Utils.GetCurrentApplicationThemeIndex(SettingsHelper.GetConfig("Theme"));

        [ObservableProperty]
        private bool _isCustomizedAccentColor = Convert.ToBoolean(SettingsHelper.GetConfig("IsCustomizedAccentColor"));

        #region AccentColorGroup
        [ObservableProperty]
        private SolidColorBrush _systemAccentColor = new();
        [ObservableProperty]
        private SolidColorBrush? _light1;
        [ObservableProperty]
        private SolidColorBrush? _light2;
        [ObservableProperty]
        private SolidColorBrush? _light3;
        [ObservableProperty]
        private SolidColorBrush? _dark1;
        [ObservableProperty]
        private SolidColorBrush? _dark2;
        [ObservableProperty]
        private SolidColorBrush? _dark3;
        #endregion AccentColorGroup

        [ObservableProperty]
        private int _currentBackdropIndex = Helpers.Utils.GetCurrentBackdropIndex(SettingsHelper.GetConfig("Backdrop"));

        public SettingsViewModel()
        {

        }

        public void OnNavigatedTo()
        {
            FileOccupancyExpander_Expanded();
        }

        public void OnNavigatedFrom()
        {

        }

        partial void OnAutoStartChanged(bool value)
        {
            SettingsHelper.SetConfig("AutoStart", value.ToString());
            AutoStartSettings.SetMeAutoStart(value);
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

        public void FileOccupancyExpander_Expanded()
        {
            if (IsFileOccupancyExpanded)
            {
                DataCount = "数据库文件已占用 " + FileOccupancy.GetFileSize(Environment.CurrentDirectory + @".\database\books.smartlibrary");
                PictureCacheCount = "缓存文件已占用 " + FileOccupancy.GetDirectorySize(Environment.CurrentDirectory + @".\pictures\");
                TempCount = "临时文件已占用 " + FileOccupancy.GetDirectorySize(Environment.CurrentDirectory + @".\temp\");
            }
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

        partial void OnIsCustomizedAccentColorChanged(bool value)
        {
            SettingsHelper.SetConfig("IsCustomizedAccentColor", value.ToString());
            if (value)
            {
                SystemAccentColor = Helpers.Utils.StringToSolidColorBrush(SettingsHelper.GetConfig("CustomizedAccentColor"));
                ApplicationAccentColorManager.Apply(SystemAccentColor.Color, Helpers.Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme")));
            }
            else
            {
                ApplicationAccentColorManager.ApplySystemAccent();
                SystemAccentColor = (SolidColorBrush)ApplicationAccentColorManager.SystemAccentBrush;
            }
            Color _color = SystemAccentColor.Color;
            Light1 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(15f, -12f));
            Light2 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(30f, -24f));
            Light3 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(45f, -36f));
            Dark1 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-5f));
            Dark2 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-10f));
            Dark3 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-15f));
        }

        public void ColorExpander_Expanded()
        {
            if (IsCustomizedAccentColor)
            {
                SystemAccentColor = Helpers.Utils.StringToSolidColorBrush(SettingsHelper.GetConfig("CustomizedAccentColor"));
            }
            else
            {
                SystemAccentColor = (SolidColorBrush)ApplicationAccentColorManager.SystemAccentBrush;
            }
            Color _color = SystemAccentColor.Color;
            Light1 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(15f, -12f));
            Light2 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(30f, -24f));
            Light3 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(45f, -36f));
            Dark1 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-5f));
            Dark2 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-10f));
            Dark3 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-15f));
        }

        [RelayCommand]
        private void OnCustomizedAccentColorChanged(string color)
        {
            if (color != SystemAccentColor.ToString())
            {
                if (IsCustomizedAccentColor)
                {
                    SystemAccentColor = Helpers.Utils.StringToSolidColorBrush(color);
                    SettingsHelper.SetConfig("CustomizedAccentColor", color);
                }
                else
                {
                    SystemAccentColor = (SolidColorBrush)ApplicationAccentColorManager.SystemAccentBrush;
                }
                Color _color = SystemAccentColor.Color;
                Light1 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(15f, -12f));
                Light2 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(30f, -24f));
                Light3 = Helpers.Utils.ColorToSolidColorBrush(_color.Update(45f, -36f));
                Dark1 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-5f));
                Dark2 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-10f));
                Dark3 = Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-15f));
                ApplicationAccentColorManager.Apply(SystemAccentColor.Color, Helpers.Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme")));
            }
        }

        partial void OnCurrentBackdropIndexChanged(int value)
        {
            if (value == 0)
            {
                SettingsHelper.SetConfig("Backdrop", "None");
            }
            else if (value == 1)
            {
                SettingsHelper.SetConfig("Backdrop", "Acrylic");
            }
            else if (value == 2)
            {
                SettingsHelper.SetConfig("Backdrop", "Mica");
            }
            else
            {
                SettingsHelper.SetConfig("Backdrop", "Tabbed");
            }
        }
    }
}