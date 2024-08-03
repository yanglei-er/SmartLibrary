using System.Configuration;

namespace Shared.Helpers
{
    public static class SettingsHelper
    {
        private static readonly Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        //设置键值
        public static void SetConfig(string key, string value)
        {
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }

        //获取键值
        public static string GetConfig(string key)
        {
            return config.AppSettings.Settings[key].Value;
        }

        public static bool GetBoolean(string key)
        {
            return Boolean.Parse(GetConfig(key));
        }

        public static int GetInt(string key)
        {
            return Int32.Parse(GetConfig(key));
        }
    }
}
