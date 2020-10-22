using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IContainsHistoricalData : IBasicWeatherRepository
    {
        Task<IEnumerable<WeatherData>> GetHistoricalData();
    }
}