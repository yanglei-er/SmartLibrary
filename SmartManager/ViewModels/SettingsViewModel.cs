using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Helpers;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Extensions;

namespace SmartManager.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _currentApplicationThemeIndex = Utils.GetCurrentApplicationThemeIndex(SettingsHelper.GetConfig("Theme"));

        [ObservableProperty]
        private bool _isCustomizedAccentColor = SettingsHelper.GetBoolean("IsCustomizedAccentColor");

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
        private int _currentBackdropIndex = Utils.GetCurrentBackdropIndex(SettingsHelper.GetConfig("Backdrop"));

        partial void OnCurrentApplicationThemeIndexChanged(int value)
        {
            //在不重启的情况下，咱不能完全更换主题
            if (value == 0)
            {
                SettingsHelper.SetConfig("Theme", "System");
                //ApplicationTheme theme = Helpers.Utils.GetUserApplicationTheme("System");
                //ApplicationThemeManager.Apply(theme, Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                //ResourceManager.UpdateTheme(theme.ToString());
            }
            else if (value == 1)
            {
                SettingsHelper.SetConfig("Theme", "Light");
                //ApplicationThemeManager.Apply(ApplicationTheme.Light, Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                //ResourceManager.UpdateTheme("Light");
            }
            else
            {
                SettingsHelper.SetConfig("Theme", "Dark");
                //ApplicationThemeManager.Apply(ApplicationTheme.Dark, Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                //ResourceManager.UpdateTheme("Dark");
            }
        }

        partial void OnIsCustomizedAccentColorChanged(bool value)
        {
            SettingsHelper.SetConfig("IsCustomizedAccentColor", value.ToString());
            if (value)
            {
                SystemAccentColor = Utils.StringToSolidColorBrush(SettingsHelper.GetConfig("CustomizedAccentColor"));
                ApplicationAccentColorManager.Apply(SystemAccentColor.Color, Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme")));
            }
            else
            {
                ApplicationAccentColorManager.ApplySystemAccent();
                SystemAccentColor = (SolidColorBrush)ApplicationAccentColorManager.SystemAccentBrush;
            }
            Color _color = SystemAccentColor.Color;
            Light1 = Utils.ColorToSolidColorBrush(_color.Update(15f, -12f));
            Light2 = Utils.ColorToSolidColorBrush(_color.Update(30f, -24f));
            Light3 = Utils.ColorToSolidColorBrush(_color.Update(45f, -36f));
            Dark1 = Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-5f));
            Dark2 = Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-10f));
            Dark3 = Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-15f));
        }

        public void ColorExpander_Expanded()
        {
            if (IsCustomizedAccentColor)
            {
                SystemAccentColor = Shared.Helpers.Utils.StringToSolidColorBrush(SettingsHelper.GetConfig("CustomizedAccentColor"));
            }
            else
            {
                SystemAccentColor = (SolidColorBrush)ApplicationAccentColorManager.SystemAccentBrush;
            }
            Color _color = SystemAccentColor.Color;
            Light1 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.Update(15f, -12f));
            Light2 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.Update(30f, -24f));
            Light3 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.Update(45f, -36f));
            Dark1 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-5f));
            Dark2 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-10f));
            Dark3 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-15f));
        }

        [RelayCommand]
        private void OnCustomizedAccentColorChanged(string color)
        {
            if (color != SystemAccentColor.ToString())
            {
                if (IsCustomizedAccentColor)
                {
                    SystemAccentColor = Shared.Helpers.Utils.StringToSolidColorBrush(color);
                    SettingsHelper.SetConfig("CustomizedAccentColor", color);
                }
                else
                {
                    SystemAccentColor = (SolidColorBrush)ApplicationAccentColorManager.SystemAccentBrush;
                }
                Color _color = SystemAccentColor.Color;
                Light1 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.Update(15f, -12f));
                Light2 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.Update(30f, -24f));
                Light3 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.Update(45f, -36f));
                Dark1 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-5f));
                Dark2 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-10f));
                Dark3 = Shared.Helpers.Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-15f));
                ApplicationAccentColorManager.Apply(SystemAccentColor.Color, Shared.Helpers.Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme")));
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
