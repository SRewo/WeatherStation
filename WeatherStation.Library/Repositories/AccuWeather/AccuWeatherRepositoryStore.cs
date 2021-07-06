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
        private Coordinates _coordinates;
        public string CityId { get; set; }
        public bool ContainsDailyForecasts { get; } = true;
        public bool ContainsHistoricalData { get; } = false;
        public bool ContainsHourlyForecasts { get; } = true;
        public string CityName { get; set; }
        public string RepositoryName { get; set; } = "AccuWeather";
        public string Language {get; private set; }
        public IWeatherRepository CurrentWeatherRepository { get; private set; }
        public IWeatherRepository DailyForecastsRepository { get; private set; }
        public IWeatherRepository HistoricalDataRepository { get; private set; }
        public IWeatherRepository HourlyForecastsRepository { get; private set; }


        public static async Task<AccuWeatherRepositoryStore> FromCityCode(string key, string cityCode, IDateProvider dateProvider, IRestClient restClient, string language)
        {
            var store = new AccuWeatherRepositoryStore(key, cityCode, dateProvider, restClient, language);
            await store.CreateRepositories(cityCode);
            return store;
        }

        public static async Task<AccuWeatherRepositoryStore> FromCityCoordinates(string key, Coordinates coordinates, IDateProvider dateProvider, IRestClient restClient, string language)
        {
            var store = new AccuWeatherRepositoryStore(key, dateProvider, restClient,language);
            await store.ChangeCity(coordinates);
            return store;
        }

        private AccuWeatherRepositoryStore(string apiKey, IDateProvider provider, IRestClient restClient, string language)
        {
            _provider = provider;
            _restClient = restClient;
            _apiKey = apiKey;
            Language = language;
        }

        private AccuWeatherRepositoryStore(string apiKey, string cityCode, IDateProvider provider, IRestClient restClient, string language) : this(apiKey, provider, restClient, language)
        {
            CityId = cityCode;
        }

        private async Task CreateRepositories(string cityCode)
        {
            HistoricalDataRepository = null;
            CurrentWeatherRepository =
                new AccuWeatherCurrentWeatherRepository(_restClient,$"currentconditions/v1/{cityCode}", _apiKey, _provider);
            HourlyForecastsRepository =
                new AccuWeatherHourlyForecastRepository(_restClient,$"forecasts/v1/hourly/12hour/{cityCode}", _apiKey, _provider);
            DailyForecastsRepository =
                new AccuWeatherDailyForecastRepository(_restClient, $"forecasts/v1/daily/5day/{cityCode}", _apiKey, _provider);
            await ChangeLanguage(Language);
        }

        public async Task ChangeCity(Coordinates coordinates)
        {
            if (!coordinates.IsValid())
                throw new InvalidCoordinatesException();

            if (Equals(coordinates, _coordinates))
                return;

            _coordinates = coordinates;
            await CreateRepositoriesFromCoordinates(coordinates);
        }

        private async Task CreateRepositoriesFromCoordinates(Coordinates coordinates)
        {
            var repository = new AccuWeatherCityDataFromGeolocation(_restClient, _apiKey, Language);
            var result = await repository.GetCityData(coordinates);
            SetCityDataProperties(result);
            await CreateRepositories(CityId);
        }

        private void SetCityDataProperties(dynamic result)
        {
            CityId = result.Key;
            CityName = result.LocalizedName;
        }

        public Task ChangeLanguage(string language)
        {
            if (Language == language)
                return Task.CompletedTask;

            Language = language;

            return ChangeLanguageInRepositories(language);
        }

        private Task ChangeLanguageInRepositories(string language)
        {
            CurrentWeatherRepository.Language = language;
            DailyForecastsRepository.Language = language;
            HourlyForecastsRepository.Language = language;

            return Task.CompletedTask;
        }
    }

}