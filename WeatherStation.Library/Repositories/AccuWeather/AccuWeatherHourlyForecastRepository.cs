using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public class AccuWeatherHourlyForecastRepository : WeatherRestRepository
    {

        public AccuWeatherHourlyForecastRepository(IRestClient client, string resourcePath, string apiKey) : base(client,resourcePath, apiKey)
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
                .SetWindSpeed((float) o.Wind.Speed.Value, WindSpeedUnit.KilometersPerHour)
                .SetChanceOfRain((int) o.PrecipitationProbability)
                .SetPrecipitationSummary((float) o.TotalLiquid.Value)
                .SetWeatherCode((int) o.WeatherIcon)
                .SetWeatherDescription((string) o.IconPhrase);
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
    }
}