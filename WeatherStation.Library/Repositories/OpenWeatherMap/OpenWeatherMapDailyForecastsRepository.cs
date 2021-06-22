using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.OpenWeatherMap
{
    public class OpenWeatherMapDailyForecastsRepository : WeatherRestRepository
    {
        public OpenWeatherMapDailyForecastsRepository(IRestClient client, string resourcePath, string apiKey, IDateProvider dateProvider) : base(client, resourcePath, apiKey, dateProvider)
        {
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("appid", ApiKey, ParameterType.QueryString);
            request.AddParameter("lang", Language, ParameterType.QueryString);
            request.AddParameter("units", "metric", ParameterType.QueryString);
            request.AddParameter("exclude", "current,minutely,hourly,alerts", ParameterType.QueryString);
            return Task.CompletedTask;
        }

        protected override IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            object weatherData = dynamicResult.daily;
            return base.CreateWeatherDataListFromResult(weatherData);
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject)
        {
            var builder = new WeatherDataBuilder();
            builder
                .SetMinTemperature((float) dynamicObject.temp.min, TemperatureScale.Celsius)
                .SetMaxTemperature((float) dynamicObject.temp.max, TemperatureScale.Celsius)
                .SetChanceOfRain((int) dynamicObject.pop)
                .SetWeatherCode((int) dynamicObject.weather[0].id)
                .SetWeatherDescription((string) dynamicObject.weather[0].description)
                .SetWindSpeed((float) dynamicObject.wind_speed, WindSpeedUnit.MetersPerSecond)
                .SetWindDirection((int) dynamicObject.wind_deg);
                

            return builder.Build();
        }
    }
}