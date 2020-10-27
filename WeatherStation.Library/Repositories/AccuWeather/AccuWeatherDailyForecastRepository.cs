using System;
using System.Collections.Generic;
using RestSharp;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public class AccuWeatherDailyForecastRepository : WeatherRestRepository
    {
        public AccuWeatherDailyForecastRepository(RestRequestHandler handler, string apiKey) : base(handler, apiKey)
        {
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic o)
        {
            var builder = new WeatherDataBuilder();
            builder.SetDate((DateTime) o.Date)
                .SetMinTemperature((float) o.Temperature.Minimum.Value, TemperatureScale.Celsius)
                .SetMaxTemperature((float) o.Temperature.Maximum.Value, TemperatureScale.Celsius)
                .SetWindDirection((int) o.Day.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Day.Wind.Speed.Value)
                .SetChanceOfRain((int) o.Day.PrecipitationProbability)
                .SetPrecipitationSummary((float) o.Day.TotalLiquid.Value)
                .SetWeatherCode((int) o.Day.Icon)
                .SetWeatherDescription((string) o.Day.IconPhrase);
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

        protected override IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            var z = (object) dynamicResult.DailyForecasts;
            return base.CreateWeatherDataListFromResult(z);
        }
    }
}