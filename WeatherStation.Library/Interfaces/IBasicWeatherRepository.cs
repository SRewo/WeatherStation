using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IBasicWeatherRepository
    {
        string RepositoryName { get; set; }
        float Latitude { get; set; }
        float Longitude { get; set; }
        Task<WeatherData> GetCurrentWeather();
    }
}