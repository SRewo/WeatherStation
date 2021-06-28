using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.Weatherbit
{
    public class WeatherbitDailyForecastRepository : WeatherRestRepository
    {
        private Coordinates _coordinates;
        public WeatherbitDailyForecastRepository(IRestClient restClient, string apiKey, Coordinates coordinates, IDateProvider dateProvider) : base(restClient, "forecast/daily", apiKey, dateProvider)
        {
            _coordinates = coordinates;
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("key", ApiKey, ParameterType.QueryString);
            request.AddParameter("lang", Language.Remove(2), ParameterType.QueryString);
            request.AddParameter("lat",_coordinates.Latitude.ToString(CultureInfo.InvariantCulture), ParameterType.QueryString);
            request.AddParameter("lon", _coordinates.Longitude.ToString(CultureInfo.InvariantCulture), ParameterType.QueryString);
            return Task.FromResult(request);
        }

        protected override IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            var d = (object) dynamicResult.data;
            return base.CreateWeatherDataListFromResult(d);
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject)
        {
            var builder = new WeatherDataBuilder();
            builder.SetMinTemperature((float) dynamicObject.min_temp, TemperatureScale.Celsius)
                .SetMaxTemperature((float) dynamicObject.max_temp, TemperatureScale.Celsius)
                .SetWeatherCode((int) dynamicObject.weather.code)
                .SetWeatherDescription((string) dynamicObject.weather.description)
                .SetChanceOfRain((int) dynamicObject.pop)
                .SetWindSpeed((float) dynamicObject.wind_spd, WindSpeedUnit.MetersPerSecond)
                .SetPressure((int) dynamicObject.slp)
                .SetDate(DateTime.Parse((string) dynamicObject.datetime));
            return builder.Build();

        }
    }
}
