using System.Collections.Generic;
using System.Threading.Tasks;
using Microcharts;
using WeatherStation.Library;

namespace WeatherStation.App.Utilities
{
    public interface IChartFactory
    {
        Task<Chart> CreateChart(WeatherDataToRainChanceChartEntryConverter converter, IEnumerable<WeatherData> data);
        Task<Chart> CreateChart(WeatherDataToTemperatureChartEntryConverter converter, IEnumerable<WeatherData> data);
    }

    public class MainViewChartFactory : IChartFactory
    {
        public async Task<Chart> CreateChart(WeatherDataToRainChanceChartEntryConverter converter, IEnumerable<WeatherData> data)
        {
            var chartHandler = new MainViewBarChartHandler(converter);
            return await chartHandler.CreateChart(data);
        }

        public async Task<Chart> CreateChart(WeatherDataToTemperatureChartEntryConverter converter, IEnumerable<WeatherData> data)
        {
            var chartHandler = new MainViewLineChartHandler(converter);
            return await chartHandler.CreateChart(data);
        }
    }
}