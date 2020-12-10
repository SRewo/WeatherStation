using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.Weatherbit
{
    public class WeatherbitCurrentWeatherRepository : WeatherRestRepository 
    {
        private (double, double) _coordinates;
        private IDateProvider _dateProvider;
        public WeatherbitCurrentWeatherRepository(IRestClient client, string apiKey, (double, double) coordinates, IDateProvider dateProvider) : base(client, "current", apiKey)
        {
            _coordinates = coordinates;
            _dateProvider = dateProvider;
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
            var z = (object) dynamicResult.data;
            return base.CreateWeatherDataListFromResult(z);
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic o)
        {
            var builder = new WeatherDataBuilder();
            _ = builder.SetDate(_dateProvider.GetActualDateTime())
                .SetTemperature((float) o.temp, TemperatureScale.Celsius)
                .SetApparentTemperature((float) o.app_temp, TemperatureScale.Celsius)
                .SetWindDirection((int) o.wind_dir)
                .SetWindSpeed((float) o.wind_spd)
                .SetPressure((int) o.pres)
                .SetHumidity((int) o.rh)
                .SetPrecipitationSummary((float) o.precip)
                .SetWeatherDescription(o.weather.description);
            return builder.Build();
        }
    }
}
