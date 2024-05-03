using IWshRuntimeLibrary;
using System.IO;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SmartLibrary.Helpers
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

    public static class AutoStartSettings
    {
        /// <summary>
        /// 快捷方式名称
        /// </summary>
        private static readonly string QuickName = "智慧图书馆";

        /// <summary>
        /// 自动获取系统自动启动目录
        /// </summary>
        private static readonly string SystemStartPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        /// <summary>
        /// 自动获取程序完整路径
        /// </summary>
        private static readonly string AppAllPath = Environment.ProcessPath ?? string.Empty;

        /// <summary>
        /// 自动获取桌面目录
        /// </summary>
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>
        /// 应用图标
        /// </summary>
        private static readonly string IconPath = Path.GetFullPath("./SmartLibrary.dll") + ", 0";

        /// <summary>
        /// 设置开机自动启动-只需要调用改方法就可以了参数里面的bool变量是控制开机启动的开关的
        /// </summary>
        /// <param name="onOff">自启开关</param>
        public static void SetMeAutoStart(bool onOff)
        {
            if (onOff)//开机启动
            {
                //获取启动路径应用程序快捷方式的路径集合
                List<string> shortcutPaths = GetQuickFromFolder(SystemStartPath, AppAllPath);
                //存在2个以快捷方式则保留一个快捷方式-避免重复多于
                if (shortcutPaths.Count >= 2)
                {
                    for (int i = 1; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
                else if (shortcutPaths.Count < 1)//不存在则创建快捷方式
                {
                    CreateShortcut(SystemStartPath, QuickName, AppAllPath, "智慧图书馆管理系统", IconPath);
                }
            }
            else//开机不启动
            {
                //获取启动路径应用程序快捷方式的路径集合
                List<string> shortcutPaths = GetQuickFromFolder(SystemStartPath, AppAllPath);
                //存在快捷方式则遍历全部删除
                if (shortcutPaths.Count > 0)
                {
                    for (int i = 0; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
            }
        }

        /// <summary>
        ///  向目标路径创建指定文件的快捷方式
        /// </summary>
        /// <param name="directory">目标目录</param>
        /// <param name="shortcutName">快捷方式名字</param>
        /// <param name="targetPath">文件完全路径</param>
        /// <param name="description">描述</param>
        /// <param name="iconLocation">图标地址</param>
        /// <returns>成功或失败</returns>
        private static bool CreateShortcut(string directory, string shortcutName, string targetPath, string? description = null, string? iconLocation = null)
        {
            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);                         //目录不存在则创建
                //添加引用 Com 中搜索 Windows Script Host Object Model
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));          //合成路径
                WshShell shell = new();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);    //创建快捷方式对象
                shortcut.TargetPath = targetPath;                                                               //指定目标路径
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);                                  //设置起始位置
                shortcut.WindowStyle = 1;                                                                       //设置运行方式，默认为常规窗口
                shortcut.Description = description;                                                             //设置备注
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;    //设置图标路径
                shortcut.Save();                                                                                //保存快捷方式
                return true;
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
                System.Windows.MessageBox.Show(temp);
            }
            return false;
        }

        /// <summary>
        /// 获取指定文件夹下指定应用程序的快捷方式路径集合
        /// </summary>
        /// <param name="directory">文件夹</param>
        /// <param name="targetPath">目标应用程序路径</param>
        /// <returns>目标应用程序的快捷方式</returns>
        private static List<string> GetQuickFromFolder(string directory, string targetPath)
        {
            List<string> tempStrs = [];
            tempStrs.Clear();
            string[] files = Directory.GetFiles(directory, "*.lnk");
            if (files == null || files.Length < 1)
            {
                return tempStrs;
            }
            for (int i = 0; i < files.Length; i++)
            {
                //files[i] = string.Format("{0}\\{1}", directory, files[i]);
                string tempStr = GetAppPathFromQuick(files[i]);
                if (tempStr == targetPath)
                {
                    tempStrs.Add(files[i]);
                }
            }
            return tempStrs;
        }

        /// <summary>
        /// 获取快捷方式的目标文件路径-用于判断是否已经开启了自动启动
        /// </summary>
        /// <param name="shortcutPath"></param>
        /// <returns></returns>
        private static string GetAppPathFromQuick(string shortcutPath)
        {
            //快捷方式文件的路径 = @"d:\Test.lnk";
            if (System.IO.File.Exists(shortcutPath))
            {
                WshShell shell = new();
                IWshShortcut shortct = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                //快捷方式文件指向的路径.Text = 当前快捷方式文件IWshShortcut类.TargetPath;
                //快捷方式文件指向的目标目录.Text = 当前快捷方式文件IWshShortcut类.WorkingDirectory;
                return shortct.TargetPath;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 根据路径删除文件-用于取消自启时从计算机自启目录删除程序的快捷方式
        /// </summary>
        /// <param name="path">路径</param>
        private static void DeleteFile(string path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// 在桌面上创建快捷方式
        /// </summary>
        /// <param name="desktopPath">桌面地址</param>
        /// <param name="appPath">应用路径</param>
        /// <param name="description">快捷方式描述</param>
        /// <param name="iconLocation">快捷方式图标</param>
        private static void CreateDesktopQuick(string desktopPath, string quickName, string appPath, string? description = null, string? iconLocation = null)
        {
            List<string> shortcutPaths = GetQuickFromFolder(desktopPath, appPath);
            //如果没有则创建
            if (shortcutPaths.Count < 1)
            {
                CreateShortcut(desktopPath, quickName, appPath, description, iconLocation);
            }
        }

        public static void CreateDesktopQuick(bool onOff)
        {
            if (onOff)
            {
                CreateDesktopQuick(DesktopPath, QuickName, AppAllPath, "智慧图书馆管理软件", IconPath);
            }
            else
            {
                //存在快捷方式则遍历全部删除
                List<string> shortcutPaths = GetQuickFromFolder(DesktopPath, AppAllPath);
                if (shortcutPaths.Count > 0)
                {
                    for (int i = 0; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
            }
        }
    }
}
