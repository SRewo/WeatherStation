using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IWeatherRepositoryStore
    {
        string RepositoryName { get;}
        string CityName { get;}
        string CityId { get; }

        IWeatherRepository CurrentWeatherRepository { get; }
        IWeatherRepository DailyForecastsRepository { get; }
        IWeatherRepository HistoricalDataRepository { get; }
        IWeatherRepository HourlyForecastsRepository { get; }

        Task ChangeCity(Coordinates coordinates);
        Task ChangeLanguage(string language);
    }
}