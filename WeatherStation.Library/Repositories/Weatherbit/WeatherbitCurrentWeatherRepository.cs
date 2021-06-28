using RestSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.Weatherbit
{
    public class WeatherbitCurrentWeatherRepository : WeatherRestRepository 
    {
        private Coordinates _coordinates;
        private IDateProvider _dateProvider;
        public WeatherbitCurrentWeatherRepository(IRestClient client, string apiKey, Coordinates coordinates, IDateProvider dateProvider) : base(client, "current/", apiKey, dateProvider)
        {
            _coordinates = coordinates;
            _dateProvider = dateProvider;
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
                .SetWindSpeed((float) o.wind_spd, WindSpeedUnit.MetersPerSecond)
                .SetPressure((int) o.slp)
                .SetHumidity((int) o.rh)
                .SetPrecipitationSummary((float) o.precip)
                .SetWeatherDescription((string) o.weather.description)
                .SetWeatherCode((int) o.weather.code);
            return builder.Build();
        }
    }
}
