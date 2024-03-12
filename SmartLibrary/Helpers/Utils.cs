using Microsoft.Win32;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SmartLibrary.Helpers
{
    public sealed class Utils
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

    public sealed class AutoStartSettings
    {
        /// <summary>
        /// 将本程序设为开启自启
        /// </summary>
        /// <param name="onOff">自启开关</param>
        /// <returns></returns>
        public static bool SetMeStart(bool onOff)
        {
            string appName = "SmartLibrary";
            string appPath = Environment.CurrentDirectory;
            bool isOk = SetAutoStart(onOff, appName, appPath);
            return isOk;
        }

        /// <summary>
        /// 将应用程序设为或不设为开机启动
        /// </summary>
        /// <param name="onOff">自启开关</param>
        /// <param name="appName">应用程序名</param>
        /// <param name="appPath">应用程序完全路径</param>
        public static bool SetAutoStart(bool onOff, string appName, string appPath)
        {
            bool isOk = true;
            //如果从没有设为开机启动设置到要设为开机启动
            if (!IsExistKey(appName) && onOff)
            {
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            //如果从设为开机启动设置到不要设为开机启动
            else if (IsExistKey(appName) && !onOff)
            {
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            return isOk;
        }

        /// <summary>
        /// 判断注册键值对是否存在，即是否处于开机启动状态
        /// </summary>
        /// <param name="keyName">键值名</param>
        /// <returns></returns>
        private static bool IsExistKey(string keyName)
        {
            try
            {
                bool _exist = false;
                RegistryKey local = Registry.LocalMachine;
                RegistryKey? runs = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (runs == null)
                {
                    RegistryKey key2 = local.CreateSubKey("SOFTWARE");
                    RegistryKey key3 = key2.CreateSubKey("Microsoft");
                    RegistryKey key4 = key3.CreateSubKey("Windows");
                    RegistryKey key5 = key4.CreateSubKey("CurrentVersion");
                    RegistryKey key6 = key5.CreateSubKey("Run");
                    runs = key6;
                }
                string[] runsName = runs.GetValueNames();
                foreach (string strName in runsName)
                {
                    if (strName.Equals(keyName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        _exist = true;
                        return _exist;
                    }
                }
                return _exist;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写入或删除注册表键值对,即设为开机启动或开机不启动
        /// </summary>
        /// <param name="isStart">是否开机启动</param>
        /// <param name="exeName">应用程序名</param>
        /// <param name="path">应用程序路径带程序名</param>
        /// <returns></returns>
        private static bool SelfRunning(bool isStart, string exeName, string path)
        {
            try
            {
                RegistryKey local = Registry.LocalMachine;
                RegistryKey? key = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null)
                {
                    local.CreateSubKey("SOFTWARE//Microsoft//Windows//CurrentVersion//Run");
                }
                if (key != null)
                {
                    //若开机自启动则添加键值对
                    if (isStart)
                    {
                        key.SetValue(exeName, path);
                        key.Close();
                    }
                    else//否则删除键值对
                    {
                        string[] keyNames = key.GetValueNames();
                        foreach (string keyName in keyNames)
                        {
                            if (keyName.Equals(exeName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                key.DeleteValue(exeName);
                                key.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
