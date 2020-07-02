using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IBasicWeatherRepository
    {
        float Latitude { get; set; }
        float Longitude { get; set; }
        Task<WeatherData> GetCurrentWeather();
    }
}