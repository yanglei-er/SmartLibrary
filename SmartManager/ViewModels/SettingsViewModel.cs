﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Helpers;
using SmartManager.Helpers;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartManager.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly UsersDb FacesDb = UsersDb.GetDatabase("faces.smartmanager");

        #region FileOccupancy
        [ObservableProperty]
        private bool _isFileOccupancyExpanded = false;
        [ObservableProperty]
        private string _dataCount = "正在计算";
        [ObservableProperty]
        private bool _isCleanDatabaseEnabled = true;
        #endregion FileOccupancy

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

        public SettingsViewModel(ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
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

        public void FileOccupancyExpander_Expanded()
        {
            if (IsFileOccupancyExpanded)
            {
                DataCount = "数据库文件已占用 " + FileOccupancy.GetFileSize(Environment.CurrentDirectory + @".\database\faces.smartmanager");
                IsCleanDatabaseEnabled = true;
            }
        }

        [RelayCommand]
        private async Task OnCleanFileOccupancyButtonClick(string parameter)
        {
            if (parameter == "CleanDatabase")
            {
                System.Media.SystemSounds.Asterisk.Play();
                if (UsersDb.IsDatabaseConnected("faces.smartmanager"))
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
                        FacesDb.CleanDatabaseAsync();
                        DataCount = "数据库文件已占用 " + FileOccupancy.GetFileSize(Environment.CurrentDirectory + @".\database\faces.smartmanager");

                        _snackbarService.Show("重置成功", "所有图书数据已清除。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                        IsCleanDatabaseEnabled = false;
                    }
                }
                else
                {
                    _snackbarService.Show("重置失败", "当前未连接任何数据库。", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
            }
        }

        partial void OnCurrentApplicationThemeIndexChanged(int value)
        {
            if (value == 0)
            {
                SettingsHelper.SetConfig("Theme", "System");
                ApplicationTheme theme = Utils.GetUserApplicationTheme("System");
                ApplicationThemeManager.Apply(theme, Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme(theme.ToString());
            }
            else if (value == 1)
            {
                SettingsHelper.SetConfig("Theme", "Light");
                ApplicationThemeManager.Apply(ApplicationTheme.Light, Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme("Light");
            }
            else
            {
                SettingsHelper.SetConfig("Theme", "Dark");
                ApplicationThemeManager.Apply(ApplicationTheme.Dark, Utils.GetUserBackdrop(SettingsHelper.GetConfig("Backdrop")));
                ResourceManager.UpdateTheme("Dark");
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
                SystemAccentColor = Utils.StringToSolidColorBrush(SettingsHelper.GetConfig("CustomizedAccentColor"));
            }
            else
            {
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

        [RelayCommand]
        private void OnCustomizedAccentColorChanged(string color)
        {
            if (color != SystemAccentColor.ToString())
            {
                if (IsCustomizedAccentColor)
                {
                    SystemAccentColor = Utils.StringToSolidColorBrush(color);
                    SettingsHelper.SetConfig("CustomizedAccentColor", color);
                }
                else
                {
                    SystemAccentColor = (SolidColorBrush)ApplicationAccentColorManager.SystemAccentBrush;
                }
                Color _color = SystemAccentColor.Color;
                Light1 = Utils.ColorToSolidColorBrush(_color.Update(15f, -12f));
                Light2 = Utils.ColorToSolidColorBrush(_color.Update(30f, -24f));
                Light3 = Utils.ColorToSolidColorBrush(_color.Update(45f, -36f));
                Dark1 = Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-5f));
                Dark2 = Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-10f));
                Dark3 = Utils.ColorToSolidColorBrush(_color.UpdateBrightness(-15f));
                ApplicationAccentColorManager.Apply(SystemAccentColor.Color, Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme")));
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
