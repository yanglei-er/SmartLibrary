using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace SmartLibrary.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ApplicationTheme _currentApplicationTheme = ApplicationThemeManager.GetAppTheme();

        public SettingsViewModel()
        {
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
