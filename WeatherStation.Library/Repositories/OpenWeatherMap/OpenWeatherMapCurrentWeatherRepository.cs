using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.OpenWeatherMap
{
    public class OpenWeatherMapCurrentWeatherRepository : WeatherRestRepository
    {
        public OpenWeatherMapCurrentWeatherRepository(IRestClient client, string resourcePath, string apiKey, IDateProvider dateProvider) : base(client, resourcePath, apiKey, dateProvider)
        {
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("appid", ApiKey, ParameterType.QueryString);
            request.AddParameter("lang", Language, ParameterType.QueryString);
            request.AddParameter("units", "metric", ParameterType.QueryString);
            request.AddParameter("exclude", "minutely,hourly,daily,alerts", ParameterType.QueryString);
            return Task.CompletedTask;
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject)
        {
            dynamic weatherData = dynamicObject.current;
            var builder = new WeatherDataBuilder();
            builder
                .SetTemperature((float) weatherData.temp, TemperatureScale.Celsius)
                .SetApparentTemperature((float) weatherData.feels_like, TemperatureScale.Celsius)
                .SetHumidity((int) weatherData.humidity)
                .SetPressure((int) weatherData.pressure)
                .SetWindSpeed((float) weatherData.wind_speed, WindSpeedUnit.MetersPerSecond)
                .SetWindDirection((int) weatherData.wind_deg)
                .SetWeatherCode((int) weatherData.weather[0].id)
                .SetWeatherDescription((string) weatherData.weather[0].description);
            return builder.Build();
        }
    }
}