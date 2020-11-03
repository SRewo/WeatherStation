using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System.Net.NetworkInformation;

namespace WeatherStation.App
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;

        #endregion


        public static string GeneralSettings
        {
            get => AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            set => AppSettings.AddOrUpdateValue(SettingsKey, value);
        }

        public static string CityName
        {
            get => AppSettings.GetValueOrDefault("CityId", string.Empty);
            set => AppSettings.AddOrUpdateValue("CityId", value);
        }

        public static float Latitude
        {
            get => AppSettings.GetValueOrDefault("Latitude", 0f);
            set => AppSettings.AddOrUpdateValue("Latitude", value);
        }

        public static float Longitude
        {
            get => AppSettings.GetValueOrDefault("Longitude", 0f);
            set => AppSettings.AddOrUpdateValue("Longitude", value);
        }

    }
}
