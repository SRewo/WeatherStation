using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Repositories.Weatherbit
{
    public class WeatherbitDailyForecastRepository : WeatherRestRepository
    {
        private (double, double) _coordinates;
        public WeatherbitDailyForecastRepository(IRestClient restClient, string apiKey, (double,double) coordinates) : base(restClient, "forecast/daily", apiKey)
        {
            _coordinates = coordinates;
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("key", ApiKey);
            request.AddParameter("lang", Language.Remove(2));
            request.AddParameter("lat",_coordinates.Item1);
            request.AddParameter("lon", _coordinates.Item2);
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
                .SetWeatherDescription((string) dynamicObject.weather.description)
                .SetChanceOfRain((int) dynamicObject.pop)
                .SetWindSpeed((int) dynamicObject.wind_spd)
                .SetPressure((int) dynamicObject.pres)
                .SetDate(DateTime.Parse((string) dynamicObject.datetime));
            return builder.Build();

        }
    }
}
