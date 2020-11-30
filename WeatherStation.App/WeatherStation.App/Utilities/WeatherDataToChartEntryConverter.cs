using System.Collections.Generic;
using System.Linq;
using Microcharts;
using SkiaSharp;
using WeatherStation.Library;

namespace WeatherStation.App.Utilities
{
    public abstract class WeatherDataToChartEntryConverter
    {
        public virtual IEnumerable<ChartEntry> ConvertCollection(IEnumerable<WeatherData> data)
        {
            return data.Select(Convert);
        }

        public abstract ChartEntry Convert(WeatherData weatherData);
    }

    public abstract class WeatherDataToTemperatureChartEntryConverter : WeatherDataToChartEntryConverter
    {
        protected SKColor GetProperColorRelativeToTemperature(Temperature temperature)
        {
            var tempValue = temperature.Value;
            if(tempValue > 35)
                return SKColor.Parse("#ffee58");
            if(tempValue > 30)
                return SKColor.Parse("#f9a825");
            if(tempValue > 25)
                return SKColor.Parse("#ef6c00");
            if(tempValue > 5)
                return SKColor.Parse("#9e9e9e");
            if(tempValue > 0)
                return SKColor.Parse("#1a237e");
            if(tempValue > -5)
                return SKColor.Parse("#1e88e5");
            return SKColor.Parse("#4dd0e1");
        }
    }

    public abstract class WeatherDataToRainChanceChartEntryConverter : WeatherDataToChartEntryConverter
    {
        protected SKColor GetProperColorRelativeToRainChance(int rainChance)
        {
            if(rainChance > 80)
                return SKColor.Parse("#0d47a1");
            if(rainChance > 65)
                return SKColor.Parse("#1976d2");
            if(rainChance > 50)
                return SKColor.Parse("#42a5f5");
            if(rainChance > 40)
                return SKColor.Parse("#90caf9");
            return SKColor.Parse("#cfd8dc");
        }
    }

    public class DailyWeatherDataToTemperatureChartEntries : WeatherDataToTemperatureChartEntryConverter
    {
        public override ChartEntry Convert(WeatherData weatherData)
        {
            return new ChartEntry(weatherData.TemperatureMax.Value)
            {
                Label = weatherData.Date.ToShortDateString(),
                ValueLabel = weatherData.TemperatureMax.ToString(),
                Color = GetProperColorRelativeToTemperature(weatherData.TemperatureMax)
            };
        }
    }

    public class DailyWeatherDataToRainChanceChartEntries : WeatherDataToRainChanceChartEntryConverter
    {
        public override ChartEntry Convert(WeatherData weatherData)
        {
            return new ChartEntry(weatherData.ChanceOfRain)
            {
                ValueLabel = $"{weatherData.ChanceOfRain}%",
                Label = weatherData.Date.ToShortDateString(),
                Color = GetProperColorRelativeToRainChance(weatherData.ChanceOfRain)
            };
        }
    }

    public class HourlyWeatherDataToTemperatureChartEntries : WeatherDataToTemperatureChartEntryConverter
    {
        public override ChartEntry Convert(WeatherData weatherData)
        {
            return new ChartEntry(weatherData.Temperature.Value)
            {
                Label = weatherData.Date.ToShortTimeString(),
                ValueLabel = weatherData.Temperature.ToString(),
                Color = GetProperColorRelativeToTemperature(weatherData.Temperature)
            };
        }
    }

    public class HourlyWeatherDataToRainChanceChartEntries : WeatherDataToRainChanceChartEntryConverter
        {
            public override ChartEntry Convert(WeatherData weatherData)
            {
                return new ChartEntry(weatherData.ChanceOfRain)
                {
                    ValueLabel = $"{weatherData.ChanceOfRain}%",
                    Label = weatherData.Date.ToShortTimeString(),
                    Color = GetProperColorRelativeToRainChance(weatherData.ChanceOfRain)
                };
            }
        }
}