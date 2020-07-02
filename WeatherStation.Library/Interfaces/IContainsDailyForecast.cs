using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IContainsDailyForecast : IBasicWeatherRepository
    {
       int DailyForecastHorizon { get; }
       Task<IEnumerable<WeatherData>> GetDailyForecast();
    }
}