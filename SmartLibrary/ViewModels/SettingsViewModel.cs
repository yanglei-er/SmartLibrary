using System.Reflection;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace SmartLibrary.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ApplicationTheme _currentApplicationTheme = ApplicationThemeManager.GetAppTheme();

        [ObservableProperty]
        private string _appVersion = string.Empty;

        [ObservableProperty]
        private string _dotNetVersion = string.Empty;

        public SettingsViewModel()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
            DotNetVersion = ".Net " + Environment.Version.ToString();
            ApplicationThemeManager.Changed += OnThemeChanged;
        }

        ~SettingsViewModel()
        {
            ApplicationThemeManager.Changed -= OnThemeChanged;
        }

        partial void OnCurrentApplicationThemeChanged(ApplicationTheme value)
        {
            ApplicationThemeManager.Apply(value);
        }

        private void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
        {
            if (CurrentApplicationTheme != currentApplicationTheme)
            {
                CurrentApplicationTheme = currentApplicationTheme;
            }
        }
    }
}