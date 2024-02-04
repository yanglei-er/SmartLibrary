using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace SmartLibrary.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ApplicationTheme _currentApplicationTheme = ApplicationTheme.Unknown;

        public SettingsViewModel()
        {
            CurrentApplicationTheme = ApplicationThemeManager.GetAppTheme();
            ApplicationThemeManager.Changed += OnThemeChanged;
        }

        ~SettingsViewModel()
        {
            ApplicationThemeManager.Changed -= OnThemeChanged;
        }

        partial void OnCurrentApplicationThemeChanged(ApplicationTheme oldValue, ApplicationTheme newValue)
        {
            ApplicationThemeManager.Apply(newValue);
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
