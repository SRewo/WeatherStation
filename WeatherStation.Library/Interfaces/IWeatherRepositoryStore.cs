using System.Threading.Tasks;
using WeatherStation.Library.Repositories;

namespace WeatherStation.Library.Interfaces
{
    public interface IWeatherRepositoryStore
    {
        string RepositoryName { get;}
        float Latitude { get;}
        float Longitude { get;}
        string CityName { get;}

        IWeatherRepository CurrentWeatherRepository { get; }
        IWeatherRepository DailyForecastsRepository { get; }
        IWeatherRepository HistoricalDataRepository { get; }
        IWeatherRepository HourlyForecastsRepository { get; }

        Task ChangeCity(float latitude, float longitude);
        Task ChangeCity(string cityName);
        Task ChangeLanguage(string languageCode);
    }
}