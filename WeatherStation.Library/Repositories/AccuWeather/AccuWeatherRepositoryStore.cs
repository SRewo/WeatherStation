using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public class AccuWeatherRepositoryStore : IWeatherRepositoryStore
    {
        private readonly string _apiKey;
        private readonly IDateProvider _provider;
        private readonly IRestClient _restClient;
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string RepositoryName { get; set; } = "AccuWeather";
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public IWeatherRepository CurrentWeatherRepository { get; private set; }
        public IWeatherRepository DailyForecastsRepository { get; private set; }
        public IWeatherRepository HistoricalDataRepository { get; private set; }
        public IWeatherRepository HourlyForecastsRepository { get; private set; }


        public static AccuWeatherRepositoryStore FromCityCode(string key, string cityCode, IDateProvider dateProvider, IRestClient restClient)
        {
            return new AccuWeatherRepositoryStore(key, cityCode, dateProvider, restClient);
        }

        public static AccuWeatherRepositoryStore FromCityCoordinates(string key, float cityLatitude, float cityLongitude, IDateProvider dateProvider, IRestClient restClient)
        {
            return new AccuWeatherRepositoryStore(key, cityLatitude, cityLongitude, dateProvider, restClient);
        }

        private AccuWeatherRepositoryStore(string apiKey, float latitude, float longitude, IDateProvider provider, IRestClient restClient)
        {
            Latitude = latitude;
            Longitude = longitude;
            _restClient = restClient;
            CreateRepositories();
        }

        private AccuWeatherRepositoryStore(string apiKey, string cityCode, IDateProvider provider, IRestClient restClient)
        {
            _apiKey = apiKey;
            _provider = provider;
            CityCode = cityCode;
            _restClient = restClient;
            CreateRepositories();
        }

        private void CreateRepositories()
        {
            var currentWeatherHandler = new RestRequestHandler(_restClient, $"currentconditions/v1/{CityCode}");
            var hourlyForecastHandler = new RestRequestHandler(_restClient, $"forecasts/v1/hourly/12hour/{CityCode}");
            var dailyForecastHandler = new RestRequestHandler(_restClient, $"forecasts/v1/daily/5day/{CityCode}");
            HistoricalDataRepository = null;
            CurrentWeatherRepository =
                new AccuWeatherCurrentWeatherRepository(currentWeatherHandler, _apiKey, _provider);
            HourlyForecastsRepository =
                new AccuWeatherHourlyForecastRepository(hourlyForecastHandler, _apiKey);
            DailyForecastsRepository =
                new AccuWeatherDailyForecastRepository(dailyForecastHandler, _apiKey);
        }

        public async Task ChangeCity(float latitude, float longitude)
        {
        }

        public async Task ChangeCity(string cityName)
        {
        }

        public async Task ChangeLanguage(string languageCode)
        {
        }

        private static string GetCityCodeFromRestResult(IRestResponse result)
        {
            return string.Empty;
        }

    }
}