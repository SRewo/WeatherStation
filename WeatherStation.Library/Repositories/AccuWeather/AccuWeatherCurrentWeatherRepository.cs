using System.Collections.Generic;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.AccuWeather
{
     public class AccuWeatherCurrentWeatherRepository : WeatherRestRepository
    {
        private readonly IDateProvider _dateProvider;

        public AccuWeatherCurrentWeatherRepository( RestRequestHandler handler,string apiKey, IDateProvider dateProvider)
            : base(handler, apiKey)
        {
            _dateProvider = dateProvider;
            Language = "pl-PL";
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject)
        {
            var builder = new WeatherDataBuilder();
            builder.SetTemperature((float) dynamicObject.Temperature.Metric.Value, TemperatureScale.Celsius)
                .SetApparentTemperature((float) dynamicObject.RealFeelTemperature.Metric.Value, TemperatureScale.Celsius)
                .SetHumidity((int) dynamicObject.RelativeHumidity)
                .SetWindDirection((int) dynamicObject.Wind.Direction.Degrees)
                .SetWindSpeed((float) dynamicObject.Wind.Speed.Metric.Value)
                .SetPressure((int) dynamicObject.Pressure.Metric.Value)
                .SetPrecipitationSummary((float) dynamicObject.PrecipitationSummary.Precipitation.Metric.Value)
                .SetWeatherCode((int) dynamicObject.WeatherIcon)
                .SetWeatherDescription((string) dynamicObject.WeatherText)
                .SetDate(_dateProvider.GetActualDateTime());
            return builder.Build();

        }
        protected override IEnumerable<Parameter> CreateRequestParameters()
        {
            var parameters = new List<Parameter>
            {
                new Parameter("apikey", ApiKey, ParameterType.QueryString),
                new Parameter("details", true, ParameterType.QueryString),
                new Parameter("metric", true, ParameterType.QueryString),
                new Parameter("language", Language, ParameterType.QueryString)
            };
            return parameters;
        }

    }
}