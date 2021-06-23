using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.OpenWeatherMap
{
    public class OpenWeatherMapDailyForecastsRepository : WeatherRestRepository
    {
        private readonly Coordinates _coordinates;

        public OpenWeatherMapDailyForecastsRepository(IRestClient client, string resourcePath, string apiKey, IDateProvider dateProvider, Coordinates coordinates) : base(client, resourcePath, apiKey, dateProvider)
        {
            _coordinates = coordinates;
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("appid", ApiKey, ParameterType.QueryString);
            request.AddParameter("lang", Language, ParameterType.QueryString);
            request.AddParameter("units", "metric", ParameterType.QueryString);
            request.AddParameter("cnt", 1, ParameterType.QueryString);
            request.AddParameter("lat", _coordinates.Latitude, ParameterType.QueryString);
            request.AddParameter("lon", _coordinates.Longitude, ParameterType.QueryString);
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
                .SetChanceOfRain((int)((float) dynamicObject.pop * 100))
                .SetWeatherCode((int) dynamicObject.weather[0].id)
                .SetWeatherDescription((string) dynamicObject.weather[0].description)
                .SetWindSpeed((float) dynamicObject.wind_speed, WindSpeedUnit.MetersPerSecond)
                .SetWindDirection((int) dynamicObject.wind_deg)
                .SetDate(GetDateTimeFromUnixTimeStamp((long) dynamicObject.dt));

            return builder.Build();
        }

        private DateTime GetDateTimeFromUnixTimeStamp(long timeStamp)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return date.AddSeconds(timeStamp);
        }
    }
}