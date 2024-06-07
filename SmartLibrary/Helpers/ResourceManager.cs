namespace SmartLibrary.Helpers
{
    public static class ResourceManager
    {
        private readonly static string currentTheme = Utils.GetUserApplicationTheme(SettingsHelper.GetConfig("Theme")).ToString().ToLower();

        public readonly static string EmptyImage = $"pack://application:,,,/Assets/DynamicPic/{currentTheme}/PictureEmpty.png";

        public static string CurrentTheme
        {
            get { return currentTheme; }
        }
    }
}