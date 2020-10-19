using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IContainsHistoricalData : IBasicWeatherRepository
    {
        int MaxHistoricalHorizon { get; }
        TimeSpan DifferenceBetweenMeasurements { get; }
        Task<IEnumerable<WeatherData>> GetHistoricalData();
    }
}