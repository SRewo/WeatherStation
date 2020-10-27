using System;
using System.Collections.Generic;
using RestSharp;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public class AccuWeatherHourlyForecastRepository : WeatherRestRepository
    {

        public AccuWeatherHourlyForecastRepository(RestRequestHandler handler, string apiKey) : base(handler, apiKey)
        {
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic o)
        {
            var builder = new WeatherDataBuilder();

            builder.SetDate((DateTime) o.DateTime)
                .SetTemperature((float) o.Temperature.Value, TemperatureScale.Celsius)
                .SetApparentTemperature((float) o.RealFeelTemperature.Value, TemperatureScale.Celsius)
                .SetHumidity((int) o.RelativeHumidity)
                .SetWindDirection((int) o.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Wind.Speed.Value)
                .SetChanceOfRain((int) o.PrecipitationProbability)
                .SetPrecipitationSummary((float) o.TotalLiquid.Value)
                .SetWeatherCode((int) o.WeatherIcon)
                .SetWeatherDescription((string) o.IconPhrase);
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