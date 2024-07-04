using System.Windows;

namespace SmartManager.Helpers
{
    public static class ResourceManager
    {
        private static string currentTheme = string.Empty;
        private static ResourceDictionary? currentThemeResource;

        public static void UpdateTheme(string theme)
        {
            currentTheme = theme.ToLower();
            Application.Current.Resources.MergedDictionaries.Remove(currentThemeResource);
            currentThemeResource = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Style/{currentTheme}.xaml") };
            Application.Current.Resources.MergedDictionaries.Add(currentThemeResource);
        }

        public static string CurrentTheme
        {
            get { return currentTheme; }
        }
    }
}