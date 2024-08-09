using CommunityToolkit.Mvvm.Messaging;
using Shared.Helpers;
using SmartLibrary.Helpers;
using System.IO;
using System.Windows.Media;
using Wpf.Ui;
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
                        Content = "您的所有图书数据将被删除，且无法恢复，您确定要继续吗?",
                        PrimaryButtonText = "是",
                        CloseButtonText = "否",
                    });
                    if (result == ContentDialogResult.Primary)
                    {
                        BooksDb.CleanDatabaseAsync();
                        DataCount = "数据库文件已占用 " + FileOccupancy.GetFileSize(Environment.CurrentDirectory + @".\database\books.smartlibrary");
                        WeakReferenceMessenger.Default.Send("refresh", "BookManage");
                        WeakReferenceMessenger.Default.Send("refresh", "Bookshelf");
                        _snackbarService.Show("重置成功", "所有图书数据已清除。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
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

        partial void OnCurrentApplicationThemeIndexChanged(int value)
        {
            if (value == 0)
            {
                SettingsHelper.SetConfig("Theme", "System");
                ApplicationTheme theme = Shared.Helpers.Utils.GetUserApplicationTheme("System");
                ApplicationThemeManager.Apply(theme, Shared.Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme(theme.ToString());
            }
            else if (value == 1)
            {
                SettingsHelper.SetConfig("Theme", "Light");
                ApplicationThemeManager.Apply(ApplicationTheme.Light, Shared.Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme("Light");
            }
            else
            {
                SettingsHelper.SetConfig("Theme", "Dark");
                ApplicationThemeManager.Apply(ApplicationTheme.Dark, Shared.Helpers.Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme("Dark");
            }
        }

        partial void OnIsCustomizedAccentColorChanged(bool value)
        {
            SettingsHelper.SetConfig("IsCustomizedAccentColor", value.ToString());
            if (value)
            {
                SystemAccentColor = Shared.Helpers.Utils.StringToSolidColorBrush(SettingsHelper.GetConfig("CustomizedAccentColor"));
                ApplicationAccentColorManager.Apply(SystemAccentColor.Color, Shared.Helpers.Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme")));
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
            }
            else
            {
                _navigationService.GetNavigationControl().FooterMenuItems.RemoveAt(0);
            }
        }

        partial void OnApiKeyTextChanged(string value)
        {
            APIHelper.SetAPIKey(ApiKey);
        }
    }
}