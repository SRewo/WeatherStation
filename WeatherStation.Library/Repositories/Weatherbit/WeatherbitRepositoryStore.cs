using RestSharp;
using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.Weatherbit
{
    public class WeatherbitRepositoryStore : IWeatherRepositoryStore
    {
        private IRestClient _restClient;
        private string _apiKey;
        private IDateProvider _dateProvider;

        public string RepositoryName => "Weatherbit";

        public string CityName { get; set; } = "";

        public string CityId {get; set; }
        public string Language { get; set; }

        public IWeatherRepository CurrentWeatherRepository { get; private set; }
        public IWeatherRepository DailyForecastsRepository { get; private set; }
        public IWeatherRepository HistoricalDataRepository { get; private set; }
        public IWeatherRepository HourlyForecastsRepository { get; private set; }

        public WeatherbitRepositoryStore(IRestClient client, string apiKey, Coordinates coordinates, IDateProvider dateProvider, string language) 
        {
            _restClient = client;
            _apiKey = apiKey;
            _dateProvider = dateProvider;
            Language = language;
            CreateRepositories(coordinates);
        }

        private void CreateRepositories(Coordinates coordinates)
        {
            if(!coordinates.IsValid())
                throw new InvalidCoordinatesException();

            CurrentWeatherRepository = new WeatherbitCurrentWeatherRepository(_restClient, _apiKey, coordinates, _dateProvider);
            DailyForecastsRepository = new WeatherbitDailyForecastRepository(_restClient, _apiKey, coordinates, _dateProvider);
            HourlyForecastsRepository = new WeatherbitHourlyForecastRepository(_restClient, _apiKey, coordinates, _dateProvider);
            HistoricalDataRepository = null;
            ChangeLanguage(Language);
        }

        public Task ChangeCity(Coordinates coordinates)
        {
            CreateRepositories(coordinates);
            return Task.CompletedTask;
        }

        public Task ChangeLanguage(string language)
        {
            Language = language;
            CurrentWeatherRepository.Language = Language;
            DailyForecastsRepository.Language = Language;
            HourlyForecastsRepository.Language = Language;
            return Task.CompletedTask;
        }
    }
}
