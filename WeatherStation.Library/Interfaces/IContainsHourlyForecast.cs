using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IContainsHourlyForecast : IBasicWeatherRepository
    {
       Task<IEnumerable<WeatherData>> GetHourlyForecast();
    }
}