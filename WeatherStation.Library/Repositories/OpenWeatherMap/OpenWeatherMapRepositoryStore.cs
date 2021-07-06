using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.OpenWeatherMap
{
    public class OpenWeatherMapRepositoryStore : IWeatherRepositoryStore
    {
        private IDateProvider _dateProvider;
        private IRestClient _restClient;
        private string _apiKey;
        private string _language;
        private Coordinates _coordinates;


        public string RepositoryName { get; } = "OpenWeatherMap";
        public string CityName { get; }
        public string CityId { get; }
        public bool ContainsDailyForecasts { get; } = true;
        public bool ContainsHistoricalData { get; } = false;
        public bool ContainsHourlyForecasts { get; } = true;
        public IWeatherRepository CurrentWeatherRepository { get; private set; }
        public IWeatherRepository DailyForecastsRepository { get; private set; }
        public IWeatherRepository HistoricalDataRepository { get; private set; }
        public IWeatherRepository HourlyForecastsRepository { get; private set; }

        public OpenWeatherMapRepositoryStore(string apiKey, IDateProvider dateProvider, IRestClient restClient,
            string language, Coordinates coordinates)
        {
            _apiKey = apiKey;
            _dateProvider = dateProvider;
            _restClient = restClient;
            _language = language.Substring(0,2);
            _coordinates = coordinates;
            CreateRepositories();
        }

        private void CreateRepositories()
        {
            CurrentWeatherRepository =
                new OpenWeatherMapCurrentWeatherRepository(_restClient, "", _apiKey, _dateProvider, _coordinates);
            DailyForecastsRepository =
                new OpenWeatherMapDailyForecastsRepository(_restClient, "", _apiKey, _dateProvider, _coordinates);
            HourlyForecastsRepository =
                new OpenWeatherMapHourlyForecastsRepository(_restClient, "", _apiKey, _dateProvider, _coordinates);
            HistoricalDataRepository = null;
            ChangeLanguage(_language);

        }

        public Task ChangeCity(Coordinates coordinates)
        {
            if (coordinates.IsValid())
                throw new InvalidCoordinatesException();
            if(Equals(_coordinates, coordinates))
                return Task.CompletedTask;

            _coordinates = coordinates;
            CreateRepositories();
            return Task.CompletedTask;
        }


        public Task ChangeLanguage(string language)
        {
            _language = language.Substring(0, 2);
            CurrentWeatherRepository.Language = _language;
            DailyForecastsRepository.Language = _language;
            HourlyForecastsRepository.Language = _language;
            return Task.CompletedTask;
        }
    }
}