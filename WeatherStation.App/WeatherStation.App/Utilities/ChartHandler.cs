using System.Collections.Generic;
using System.Threading.Tasks;
using Microcharts;
using SkiaSharp;
using WeatherStation.Library;

namespace WeatherStation.App.Utilities
{
    public abstract class ChartHandler<T> where T : Chart
    {
        protected WeatherDataToChartEntryConverter Converter { get; set; }
        public T Chart { get; set; }

        protected ChartHandler(WeatherDataToChartEntryConverter converter)
        {
            Converter = converter;
        }

       public async Task<T> CreateChart(IEnumerable<WeatherMessage> data)
       {
           var entries = Converter.ConvertCollection(data);
           Chart = await CreateChartWithEntries(entries);
           return Chart;
       }

       protected abstract Task<T> CreateChartWithEntries(IEnumerable<ChartEntry> entries);
    }

    public class MainViewBarChartHandler : ChartHandler<BarChart>
    {
        public MainViewBarChartHandler(WeatherDataToChartEntryConverter converter) : base(converter)
        {
        }

        protected override Task<BarChart> CreateChartWithEntries(IEnumerable<ChartEntry> entries)
        {
            var chart = new BarChart
            {
                Entries = entries,
                MinValue = 0,
                MaxValue = 100,
                ValueLabelOrientation = Orientation.Horizontal,
                LabelOrientation = Orientation.Horizontal,
                LabelTextSize = 20
            };
            return Task.FromResult(chart);
        }
    }

    public class MainViewLineChartHandler : ChartHandler<LineChart>
    {
        public MainViewLineChartHandler(WeatherDataToChartEntryConverter converter) : base(converter)
        {
        }

        protected override Task<LineChart> CreateChartWithEntries(IEnumerable<ChartEntry> entries)
        {
            var chart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Straight,
                ValueLabelOrientation = Orientation.Horizontal,
                LineAreaAlpha = 0,
                BackgroundColor = SKColor.Empty,
                LabelOrientation = Orientation.Horizontal,
                LabelTextSize = 20,
                LineSize = 10,
                PointMode = PointMode.Circle,
                PointSize = 18
            };
            return Task.FromResult(chart);
        }
    }
}