using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace WeatherStation.Library.Repositories.Weatherbit
{
    public class WeatherbitHourlyForecastRepository : WeatherRestRepository
    { 
        private Coordinates _coordinates;

        public WeatherbitHourlyForecastRepository(IRestClient restClient, string apiKey, Coordinates coordinates) : base(restClient, "forecast/hourly", apiKey)
        {
            _coordinates = coordinates;
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("key", ApiKey, ParameterType.QueryString);
            request.AddParameter("lang", Language.Remove(2), ParameterType.QueryString);
            request.AddParameter("lat",_coordinates.Latitude.ToString(CultureInfo.InvariantCulture), ParameterType.QueryString);
            request.AddParameter("lon", _coordinates.Longitude.ToString(CultureInfo.InvariantCulture), ParameterType.QueryString);
            request.AddParameter("hours", 24,ParameterType.QueryString);
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
            builder.SetTemperature((float) dynamicObject.temp, TemperatureScale.Celsius)
                .SetApparentTemperature((float) dynamicObject.app_temp, TemperatureScale.Celsius)
                .SetWeatherCode((int) dynamicObject.weather.code)
                .SetWeatherDescription((string) dynamicObject.weather.description)
                .SetChanceOfRain((int) dynamicObject.pop)
                .SetWindSpeed((float) dynamicObject.wind_spd, WindSpeedUnit.MetersPerSecond)
                .SetPressure((int) dynamicObject.slp)
                .SetDate((DateTime) dynamicObject.timestamp_local);
            return builder.Build();

        }
    }
}
