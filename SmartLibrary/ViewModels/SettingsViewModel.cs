using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using Shared.Appearance;
using Shared.Helpers;
using SmartLibrary.Helpers;
using System.IO;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartLibrary.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly BooksDb BooksDb = BooksDb.GetDatabase("books.smartlibrary");
        private readonly UsersDb UsersDb = UsersDb.GetDatabase("users.smartmanager");

        [ObservableProperty]
        private bool _autoStart = SettingsHelper.GetBoolean("AutoStart");

        [ObservableProperty]
        private bool _autoStartMinimized = SettingsHelper.GetBoolean("AutoStartMinimized");

        [ObservableProperty]
        private bool _trayEnabled = SettingsHelper.GetBoolean("TrayEnabled");

        [ObservableProperty]
        private bool _autoCheckUpdate = SettingsHelper.GetBoolean("AutoCheckUpdate");

        #region FileOccupancy
        [ObservableProperty]
        private bool _isFileOccupancyExpanded = false;
        [ObservableProperty]
        private string _dataCount = "正在计算";
        [ObservableProperty]
        private string _pictureCacheCount = "正在计算";
        [ObservableProperty]
        private string _tempCount = "正在计算";
        [ObservableProperty]
        private bool _isCleanDatabaseEnabled = true;
        [ObservableProperty]
        private bool _isCleanPictureCacheEnabled = true;
        [ObservableProperty]
        private bool _isCleanTempEnabled = true;
        #endregion FileOccupancy

        [ObservableProperty]
        private List<string> _devicesName = [.. FaceRecognition.SystemCameraDevices.Keys];

        [ObservableProperty]
        private int _devicesIndex = 0;

        [ObservableProperty]
        private bool _isFlyoutOpen = false;

        [ObservableProperty]
        private string _flyoutText = string.Empty;

        [ObservableProperty]
        private int _timeOut = SettingsHelper.GetInt("TimedOut");

        [ObservableProperty]
        private int _currentApplicationThemeIndex = Shared.Helpers.Utils.GetCurrentApplicationThemeIndex(SettingsHelper.GetConfig("Theme"));

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
        private int _currentBackdropIndex = Shared.Helpers.Utils.GetCurrentBackdropIndex(SettingsHelper.GetConfig("Backdrop"));

        [ObservableProperty]
        private bool _isAdministrator = SettingsHelper.GetBoolean("IsAdministrator");

        [ObservableProperty]
        private string _apiKey = string.Empty;

        [ObservableProperty]
        private string _apiKeyText = APIHelper.GetAPIKey();

        public SettingsViewModel(INavigationService navigationService, IContentDialogService contentDialogService, ISnackbarService snackbarService)
        {
            _navigationService = navigationService;
            _contentDialogService = contentDialogService;
            _snackbarService = snackbarService;
            int _deviceIndex = SettingsHelper.GetInt("DeviceIndex");
            if(_deviceIndex > DevicesName.Count)
            {
                DevicesIndex = 0;
                SettingsHelper.SetConfig("DeviceIndex", "0");
            }
            else
            {
                DevicesIndex = _deviceIndex;
            }
        }

        public Task OnNavigatedToAsync()
        {
            FileOccupancyExpander_Expanded();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
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
                DataCount = "数据库文件已占用 " + FileOccupancy.GetDirectorySize(Environment.CurrentDirectory + @".\database\");
                PictureCacheCount = "缓存文件已占用 " + FileOccupancy.GetDirectorySize(Environment.CurrentDirectory + @".\pictures\");
                TempCount = "临时文件已占用 " + FileOccupancy.GetDirectorySize(Environment.CurrentDirectory + @".\temp\");

                IsCleanDatabaseEnabled = true;
                IsCleanPictureCacheEnabled = true;
                IsCleanTempEnabled = true;
            }
        }

        [RelayCommand]
        private async Task OnCleanFileOccupancyButtonClick(string parameter)
        {
            if (parameter == "CleanDatabase")
            {
                System.Media.SystemSounds.Asterisk.Play();
                if (BooksDb.IsDatabaseConnected("books.smartlibrary"))
                {
                    ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                    {
                        Title = "重置数据库",
                        Content = "您的所有数据将被删除，且无法恢复，您确定要继续吗?",
                        PrimaryButtonText = "是",
                        CloseButtonText = "否",
                    });
                    if (result == ContentDialogResult.Primary)
                    {
                        BooksDb.CleanDatabaseAsync();
                        UsersDb.CleanDatabaseAsync();
                        DataCount = "数据库文件已占用 " + FileOccupancy.GetFileSize(Environment.CurrentDirectory + @".\database\books.smartlibrary");
                        WeakReferenceMessenger.Default.Send("refresh", "BookManage");
                        WeakReferenceMessenger.Default.Send("refresh", "Bookshelf");
                        _snackbarService.Show("重置成功", "所有数据已清除。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                        IsCleanDatabaseEnabled = false;
                    }
                }
                else
                {
                    _snackbarService.Show("重置失败", "当前未连接任何数据库。", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
            }
            else if (parameter == "CleanPictureCache")
            {
                string path = Environment.CurrentDirectory + @".\pictures\";
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    PictureCacheCount = "缓存文件已占用 " + FileOccupancy.GetDirectorySize(Environment.CurrentDirectory + @".\pictures\");
                    _snackbarService.Show("清除成功", $"已清除所有图书图片缓存。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
                else
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    _snackbarService.Show("清除失败", "图书图片缓存文件夹不存在。", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
                IsCleanPictureCacheEnabled = false;
            }
            else
            {
                string path = Environment.CurrentDirectory + @".\temp\";
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    TempCount = "临时文件已占用 " + FileOccupancy.GetDirectorySize(Environment.CurrentDirectory + @".\temp\");
                    _snackbarService.Show("清除成功", $"已清除所有临时缓存。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
                else
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    _snackbarService.Show("清除失败", "临时缓存文件夹不存在。", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
                IsCleanTempEnabled = false;
            }
        }

        [RelayCommand]
        private void OnRefreshDeviceButtonClick()
        {
            DevicesName = [.. FaceRecognition.SystemCameraDevices.Keys];
        }

        partial void OnDevicesIndexChanged(int value)
        {
            SettingsHelper.SetConfig("DeviceIndex", DevicesIndex.ToString());
            WeakReferenceMessenger.Default.Send(DevicesIndex.ToString(), "deviceRefresh");
        }

        partial void OnTimeOutChanged(int value)
        {
           if (value > 15)
            {
                FlyoutText = $"等待时长不能超过15秒";
                IsFlyoutOpen = true;
                TimeOut = 15;
            }
            else if (value < 1)
            {
                FlyoutText = $"等待时长至少为1秒";
                IsFlyoutOpen = true;
                TimeOut = 1;
            }
            else if (value > 1 && value < 15)
            {
                if (IsFlyoutOpen)
                {
                    IsFlyoutOpen = false;
                }
            }
        }

        public void SetTimeOut(string timeOut)
        {
            if(string.IsNullOrEmpty(timeOut))
            {
                TimeOut = 5;
            }
            if (IsFlyoutOpen)
            {
                IsFlyoutOpen = false;
            }
            SettingsHelper.SetConfig("TimedOut", TimeOut.ToString());
        }

        partial void OnCurrentApplicationThemeIndexChanged(int value)
        {
            if (value == 0)
            {
                SettingsHelper.SetConfig("Theme", "System");
                ApplicationTheme theme = Shared.Helpers.Utils.GetUserApplicationTheme("System");
                ThemeManager.Apply(theme, Shared.Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme(theme.ToString());
            }
            else if (value == 1)
            {
                SettingsHelper.SetConfig("Theme", "Light");
                ThemeManager.Apply(ApplicationTheme.Light, Shared.Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme("Light");
            }
            else
            {
                SettingsHelper.SetConfig("Theme", "Dark");
                ThemeManager.Apply(ApplicationTheme.Dark, Shared.Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme("Dark");
            }
        }

        partial void OnIsCustomizedAccentColorChanged(bool value)
        {
            SettingsHelper.SetConfig("IsCustomizedAccentColor", value.ToString());
            if (value)
            {
                SystemAccentColor = Shared.Helpers.Utils.StringToSolidColorBrush(SettingsHelper.GetConfig("CustomizedAccentColor"));
            }
            else
            {
                ApplicationAccentColorManager.ApplySystemAccent();
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
            }
        }

        partial void OnCurrentBackdropIndexChanged(int value)
        {
            ApplicationTheme theme = Shared.Helpers.Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme"));
            if (value == 0)
            {
                SettingsHelper.SetConfig("Backdrop", "None");
                BackgroundManager.UpdateBackground(UiApplication.Current.MainWindow, theme, WindowBackdropType.None);
            }
            else if (value == 1)
            {
                SettingsHelper.SetConfig("Backdrop", "Acrylic");
                BackgroundManager.UpdateBackground(UiApplication.Current.MainWindow, theme, WindowBackdropType.Acrylic);
            }
            else if (value == 2)
            {
                SettingsHelper.SetConfig("Backdrop", "Mica");
                BackgroundManager.UpdateBackground(UiApplication.Current.MainWindow, theme, WindowBackdropType.Mica);
            }
            else
            {
                SettingsHelper.SetConfig("Backdrop", "Tabbed");
                BackgroundManager.UpdateBackground(UiApplication.Current.MainWindow, theme, WindowBackdropType.Tabbed);
            }
        }

        partial void OnIsAdministratorChanged(bool value)
        {
            SettingsHelper.SetConfig("IsAdministrator", value.ToString());
            if (value)
            {
                _navigationService.GetNavigationControl().FooterMenuItems.Insert(0, new NavigationViewItem()
                {
                    Content = "管理",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Apps24 },
                    TargetPageType = typeof(Views.Pages.BookManage)
                });
                _navigationService.GetNavigationControl().FooterMenuItems.Insert(0, new NavigationViewItem()
                {
                    Content = "用户",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Accessibility24 },
                    TargetPageType = typeof(Views.Pages.UserManage)
                });
            }
            else
            {
                _navigationService.GetNavigationControl().FooterMenuItems.RemoveAt(0);
                _navigationService.GetNavigationControl().FooterMenuItems.RemoveAt(0);
            }
        }

        partial void OnApiKeyTextChanged(string value)
        {
            APIHelper.SetAPIKey(ApiKey);
        }

        public void CopyMailAddress()
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetText("zhao.yanglei@foxmail.com");
                _snackbarService.Show("复制成功", "邮箱地址已复制到剪贴板", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
            }
            catch (Exception e)
            {
                _snackbarService.Show("复制失败", $"{e.Message}", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
            }
        }
    }
}