using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.OpenWeatherMap
{
    public class OpenWeatherMapCurrentWeatherRepository : WeatherRestRepository
    {
        private readonly Coordinates _coordinates;

        public OpenWeatherMapCurrentWeatherRepository(IRestClient client, string resourcePath, string apiKey, IDateProvider dateProvider, Coordinates coordinates) : base(client, resourcePath, apiKey, dateProvider)
        {
            _coordinates = coordinates;
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("appid", ApiKey, ParameterType.QueryString);
            request.AddParameter("lang", Language, ParameterType.QueryString);
            request.AddParameter("units", "metric", ParameterType.QueryString);
            request.AddParameter("lat", _coordinates.Latitude, ParameterType.QueryString);
            request.AddParameter("cnt", 1, ParameterType.QueryString);
            request.AddParameter("lon", _coordinates.Longitude, ParameterType.QueryString);
            request.AddParameter("exclude", "minutely,hourly,daily,alerts", ParameterType.QueryString);
            return Task.CompletedTask;
        }

        protected override IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            object weatherData = dynamicResult.current;
            return base.CreateWeatherDataListFromResult(weatherData);
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject)
        {
            var builder = new WeatherDataBuilder();
            builder
                .SetTemperature((float) dynamicObject.temp, TemperatureScale.Celsius)
                .SetApparentTemperature((float) dynamicObject.feels_like, TemperatureScale.Celsius)
                .SetHumidity((int) dynamicObject.humidity)
                .SetPressure((int) dynamicObject.pressure)
                .SetWindSpeed((float) dynamicObject.wind_speed, WindSpeedUnit.MetersPerSecond)
                .SetWindDirection((int) dynamicObject.wind_deg)
                .SetWeatherCode((int) dynamicObject.weather[0].id)
                .SetWeatherDescription((string) dynamicObject.weather[0].description)
                .SetDate(DateProvider.GetActualDateTime());
            return builder.Build();
        }
    }
}