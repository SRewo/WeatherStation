using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IWeatherRepositoryStore
    {
        string RepositoryName { get;}
        string CityName { get;}
        string CityId { get; }
        bool ContainsDailyForecasts { get; }
        bool ContainsHistoricalData { get; }
        bool ContainsHourlyForecasts { get; }

        IWeatherRepository CurrentWeatherRepository { get; }
        IWeatherRepository DailyForecastsRepository { get; }
        IWeatherRepository HistoricalDataRepository { get; }
        IWeatherRepository HourlyForecastsRepository { get; }

        Task ChangeCity(Coordinates coordinates);
        Task ChangeLanguage(string language);
    }
}