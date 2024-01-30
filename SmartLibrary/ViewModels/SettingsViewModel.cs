using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace SmartLibrary.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private Wpf.Ui.Appearance.ApplicationTheme _currentApplicationTheme = Wpf.Ui.Appearance.ApplicationTheme.Unknown;

        public SettingsViewModel()
        {
            if (!_isInitialized)
            {
                CurrentApplicationTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();

                ApplicationThemeManager.Changed += OnThemeChanged;

                _isInitialized = true;
            }
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
