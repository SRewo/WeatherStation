using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public class AccuWeatherCurrentWeatherRepository : WeatherRestRepository
    {
        private readonly IDateProvider _dateProvider;

        public AccuWeatherCurrentWeatherRepository(IRestClient client, string resourcePath,string apiKey, IDateProvider dateProvider)
            : base(client, resourcePath, apiKey, dateProvider)
        {
            _dateProvider = dateProvider;
            Language = "pl-PL";
        }

        protected override WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject)
        {
            var builder = new WeatherDataBuilder();
            builder.SetTemperature((float) dynamicObject.Temperature.Metric.Value, TemperatureScale.Celsius)
                .SetApparentTemperature((float) dynamicObject.RealFeelTemperature.Metric.Value, TemperatureScale.Celsius)
                .SetHumidity((int) dynamicObject.RelativeHumidity)
                .SetWindDirection((int) dynamicObject.Wind.Direction.Degrees)
                .SetWindSpeed((float) dynamicObject.Wind.Speed.Metric.Value, WindSpeedUnit.KilometersPerHour)
                .SetPressure((int) dynamicObject.Pressure.Metric.Value)
                .SetPrecipitationSummary((float) dynamicObject.PrecipitationSummary.Precipitation.Metric.Value)
                .SetWeatherCode((int) dynamicObject.WeatherIcon)
                .SetWeatherDescription((string) dynamicObject.WeatherText)
                .SetDate(_dateProvider.GetActualDateTime());
            return builder.Build();

        }
        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("apikey", ApiKey, ParameterType.QueryString);
            request.AddParameter("details", true, ParameterType.QueryString);
            request.AddParameter("metric", true, ParameterType.QueryString);
            request.AddParameter("language", Language, ParameterType.QueryString);
            return Task.CompletedTask;
        }

    }
}