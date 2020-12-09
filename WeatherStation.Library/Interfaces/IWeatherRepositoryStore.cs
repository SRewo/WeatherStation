using System.Threading.Tasks;
using WeatherStation.Library.Repositories;

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

        Task ChangeCity(double latitude, double longitude);
        Task ChangeLanguage(string language);
    }
}