using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.OpenWeatherMap
{
    public class OpenWeatherMapHourlyForecastsRepository : WeatherRestRepository
    {
        private readonly Coordinates _coordinates;

        public OpenWeatherMapHourlyForecastsRepository(IRestClient client, string resourcePath, string apiKey, IDateProvider dateProvider, Coordinates coordinates) : base(client, resourcePath, apiKey, dateProvider)
        {
            _coordinates = coordinates;
        }

        protected override IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            object weatherData = dynamicResult.hourly;
            return base.CreateWeatherDataListFromResult(weatherData);
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("appid", ApiKey, ParameterType.QueryString);
            request.AddParameter("lang", Language, ParameterType.QueryString);
            request.AddParameter("units", "metric", ParameterType.QueryString);
            request.AddParameter("cnt", 1, ParameterType.QueryString);
            request.AddParameter("lat", _coordinates.Latitude, ParameterType.QueryString);
            request.AddParameter("lon", _coordinates.Longitude, ParameterType.QueryString);
            request.AddParameter("exclude", "minutely,current,daily,alerts", ParameterType.QueryString);
            return Task.CompletedTask;
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject)
        {
            var builder = new WeatherDataBuilder();
            builder
                .SetApparentTemperature((float) dynamicObject.feels_like, TemperatureScale.Celsius)
                .SetTemperature((float) dynamicObject.temp, TemperatureScale.Celsius)
                .SetHumidity((int) dynamicObject.humidity)
                .SetPressure((int) dynamicObject.pressure)
                .SetDate(GetLocalDateTimeFromResponse((object) dynamicObject))
                .SetChanceOfRain((int)((float) dynamicObject.pop * 100))
                .SetWindSpeed((float) dynamicObject.wind_speed, WindSpeedUnit.MetersPerSecond)
                .SetWindDirection((int) dynamicObject.wind_deg)
                .SetWeatherCode((int) dynamicObject.weather[0].id)
                .SetWeatherDescription((string) dynamicObject.weather[0].description);
            return builder.Build();
        }

        private DateTime GetLocalDateTimeFromResponse(dynamic dynamicObject)
        {
            var date = new DateTime(1970,1,1,0,0,0, DateTimeKind.Utc);
            date = date.AddSeconds((long) dynamicObject.dt);
            return date.ToLocalTime();
        }
    }
}