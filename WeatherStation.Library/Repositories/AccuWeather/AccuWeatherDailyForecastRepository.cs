using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public class AccuWeatherDailyForecastRepository : WeatherRestRepository
    {
        public AccuWeatherDailyForecastRepository(IRestClient client, string resourcePath, string apiKey) : base(client, resourcePath, apiKey)
        {
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic o)
        {
            var builder = new WeatherDataBuilder();
            builder.SetDate((DateTime) o.Date)
                .SetMinTemperature((float) o.Temperature.Minimum.Value, TemperatureScale.Celsius)
                .SetMaxTemperature((float) o.Temperature.Maximum.Value, TemperatureScale.Celsius)
                .SetWindDirection((int) o.Day.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Day.Wind.Speed.Value, WindSpeedUnit.KilometersPerHour)
                .SetChanceOfRain((int) o.Day.PrecipitationProbability)
                .SetPrecipitationSummary((float) o.Day.TotalLiquid.Value)
                .SetWeatherCode((int) o.Day.Icon)
                .SetWeatherDescription((string) o.Day.IconPhrase);
            return builder.Build();
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("apikey", ApiKey, ParameterType.QueryString);
            request.AddParameter("details", true, ParameterType.QueryString);
            request.AddParameter("metric", true, ParameterType.QueryString);
            request.AddParameter("language", Language, ParameterType.QueryString);
            return Task.CompletedTask;
        }

        protected override IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            var z = (object) dynamicResult.DailyForecasts;
            return base.CreateWeatherDataListFromResult(z);
        }
    }
}