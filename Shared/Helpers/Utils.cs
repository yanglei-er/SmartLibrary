﻿using System.Runtime.InteropServices;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Shared.Helpers
{
    public static class Utils
    {
        public static int GetCurrentApplicationThemeIndex(string theme)
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

        public static ApplicationTheme GetUserApplicationTheme(string theme)
        {
            if (theme == "System")
            {
                SystemTheme systemTheme = ApplicationThemeManager.GetSystemTheme();

                if (systemTheme is SystemTheme.Dark or SystemTheme.CapturedMotion or SystemTheme.Glow)
                {
                    return ApplicationTheme.Dark;
                }
                else
                {
                    return ApplicationTheme.Light;
                }
            }
            else if (theme == "Light")
            {
                return ApplicationTheme.Light;
            }
            else
            {
                return ApplicationTheme.Dark;
            }
        }

        public static int GetCurrentBackdropIndex(string backdrop)
        {
            if (backdrop == "None")
            {
                return 0;
            }
            else if (backdrop == "Acrylic")
            {
                return 1;
            }
            else if (backdrop == "Mica")
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        public static WindowBackdropType GetUserBackdrop(string backdrop)
        {
            if (backdrop == "None")
            {
                return WindowBackdropType.None;
            }
            else if (backdrop == "Acrylic")
            {
                return WindowBackdropType.Acrylic;
            }
            else if (backdrop == "Mica")
            {
                return WindowBackdropType.Mica;
            }
            else
            {
                return WindowBackdropType.Tabbed;
            }
        }

        public static SolidColorBrush StringToSolidColorBrush(string color)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public static SolidColorBrush ColorToSolidColorBrush(Color color)
        {
            return new SolidColorBrush(color);
        }

        public static Color StringToColor(string color)
        {
            return (Color)ColorConverter.ConvertFromString(color);
        }
    }

    public class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
}
