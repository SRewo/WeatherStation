using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microcharts;
using WeatherStation.Library;

namespace WeatherStation.App.Utilities
{
    public static class MainViewChartFactory
    {
        public static async Task<Chart> CreateRainChanceChart(WeatherDataToRainChanceChartEntryConverter converter, IEnumerable<WeatherData> data)
        {
            var chartHandler = new MainViewBarChartHandler(converter);
            if(data.Count() > 12)
                data = data.Take(12);
            return await chartHandler.CreateChart(data);
        }

        public static async Task<Chart> CreateTemperatureChart(WeatherDataToTemperatureChartEntryConverter converter, IEnumerable<WeatherData> data)
        {
            var chartHandler = new MainViewLineChartHandler(converter);
            if(data.Count() > 12)
                data = data.Take(12);
            return await chartHandler.CreateChart(data);
        }
    }
}