using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IContainsHourlyForecast : IBasicWeatherRepository
    {
       int HourlyForecastHorizon { get; }
       Task<IEnumerable<WeatherData>> GetHourlyForecast();
    }
}