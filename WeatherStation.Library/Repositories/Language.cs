namespace WeatherStation.Library.Repositories
{
    public enum Language
    {
        Polish,
        English
    }

    public static class LanguageConverter
    {
        public static string ConvertEnumToLanguageCode(Language language)
        {
            switch (language)
            {
                case Language.Polish:
                    return "pl-PL";
                case Language.English:
                    return "en-EN";
                default:
                    return string.Empty;
            }
        }
    }
}