using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories
{
    public class AccuWeatherRepository : IWeatherRepository ,IContainsDailyForecast, IContainsHourlyForecast, IContainsHistoricalData
    {
        private readonly string _apiKey;
        private readonly IDateProvider _dateProvider;
        private readonly IRestClient _restClient;
        public string CityCode { get; set; }
        public string Language { get; set; } = "pl-PL";
        public string RepositoryName { get; set; } = "AccuWeather";
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public int MaxHistoricalHorizon { get; } = 24;
        public TimeSpan DifferenceBetweenMeasurements { get; } = TimeSpan.FromHours(1);
        public int DailyForecastHorizon { get; } = 5;
        public IContainsDailyForecast DailyRepository => this;
        public IContainsHistoricalData HistoricalRepository => this;
        public IContainsHourlyForecast HourlyRepository => this;
        public int HourlyForecastHorizon { get; } = 12;


        public static AccuWeatherRepository FromCityCode(string key, string cityCode, IDateProvider dateProvider, IRestClient restClient)
        {
            return new AccuWeatherRepository(key, cityCode, dateProvider, restClient);
        }

        public static AccuWeatherRepository FromCityCoordinates(string key, float cityLatitude, float cityLongitude, IDateProvider dateProvider, IRestClient restClient)
        {
            return new AccuWeatherRepository(key, cityLatitude, cityLongitude, dateProvider, restClient);
        }

        private AccuWeatherRepository(string apiKey, float latitude, float longitude, IDateProvider provider, IRestClient restClient)
        {
            _apiKey = apiKey;
            Latitude = latitude;
            Longitude = longitude;
            _dateProvider = provider;
            _restClient = restClient;
        }

        private AccuWeatherRepository(string apiKey, string cityCode, IDateProvider provider, IRestClient restClient)
        {
            _apiKey = apiKey;
            CityCode = cityCode;
            _dateProvider = provider;
            _restClient = restClient;
        }

        public async Task<WeatherData> GetCurrentWeather()
        {
            await SetCityCodeIfEmpty();
            var dynamicResult = GetDataFromApi($"currentconditions/v1/{CityCode}");
            return BuildCurrentWeatherDataFromDynamicObject(dynamicResult);
        }


        private WeatherData BuildCurrentWeatherDataFromDynamicObject(dynamic dynamicResult)
        {
            var o = dynamicResult[0];
            var builder = new WeatherDataBuilder();
            builder.SetTemperature((float) o.Temperature.Metric.Value, TemperatureUnit.Celcius)
                .SetApparentTemperature((float) o.RealFeelTemperature.Metric.Value, TemperatureUnit.Celcius)
                .SetHumidity((int) o.RelativeHumidity)
                .SetWindDirection((int) o.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Wind.Speed.Metric.Value)
                .SetPressure((int) o.Pressure.Metric.Value)
                .SetPrecipitationSummary((float) o.PrecipitationSummary.Precipitation.Metric.Value)
                .SetWeatherCode((int) o.WeatherIcon)
                .SetWeatherDescription((string) o.WeatherText)
                .SetDate(_dateProvider.GetActualDateTime());
            return builder.Build();
        }

        public async Task<IEnumerable<WeatherData>> GetDailyForecast()
        {
            await SetCityCodeIfEmpty();
            var dynamicResult = GetDataFromApi($"forecasts/v1/daily/5day/{CityCode}");
            return CreateDailyForecasts(dynamicResult);
        }

        private static IEnumerable<WeatherData> CreateDailyForecasts(dynamic dynamicResult)
        {
            var list = new List<WeatherData>();
            foreach (var o in dynamicResult.DailyForecasts)
                list.Add(BuildDailyForecastDataFromDynamicObject(o));
            return list;
        }

        private static WeatherData BuildDailyForecastDataFromDynamicObject(dynamic o)
        {
            var builder = new WeatherDataBuilder();
            builder.SetDate((DateTime) o.Date)
                .SetMinTemperature((float) o.Temperature.Minimum.Value, TemperatureUnit.Celcius)
                .SetMaxTemperature((float) o.Temperature.Maximum.Value, TemperatureUnit.Celcius)
                .SetWindDirection((int) o.Day.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Day.Wind.Speed.Value)
                .SetChanceOfRain((int) o.Day.PrecipitationProbability)
                .SetPrecipitationSummary((float) o.Day.TotalLiquid.Value)
                .SetWeatherCode((int) o.Day.Icon)
                .SetWeatherDescription((string) o.Day.IconPhrase);
            return builder.Build();
        }


        public async Task<IEnumerable<WeatherData>> GetHourlyForecast()
        {
            await SetCityCodeIfEmpty();
            var dynamicResult = await GetDataFromApi($"forecasts/v1/hourly/12hour/{CityCode}");
            return CreateHourlyForecasts(dynamicResult);
        }

        private async Task<dynamic> GetDataFromApi(string resourcePath)
        {
            var query = CreateRestRequest(resourcePath);
            var result = await ExecuteRestRequest(query);
            return ConvertRestResultToDynamicObject(result);
        }

        private static IEnumerable<WeatherData> CreateHourlyForecasts(dynamic dynamicResult)
        {
            var list = new List<WeatherData>();

            foreach (var o in dynamicResult)
                list.Add(BuildHourlyForecastDataFromDynamicObject(o));

            return list;
        }

        private static WeatherData BuildHourlyForecastDataFromDynamicObject(dynamic o)
        {
            var builder = new WeatherDataBuilder();

            builder.SetDate((DateTime) o.DateTime)
                .SetTemperature((float) o.Temperature.Value, TemperatureUnit.Celcius)
                .SetApparentTemperature((float) o.RealFeelTemperature.Value, TemperatureUnit.Celcius)
                .SetHumidity((int) o.RelativeHumidity)
                .SetWindDirection((int) o.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Wind.Speed.Value)
                .SetChanceOfRain((int) o.PrecipitationProbability)
                .SetPrecipitationSummary((float) o.TotalLiquid.Value)
                .SetWeatherCode((int) o.WeatherIcon)
                .SetWeatherDescription((string) o.IconPhrase);
            return builder.Build();
        }

        public async Task<IEnumerable<WeatherData>> GetHistoricalData()
        {
             await SetCityCodeIfEmpty();
             var dynamicResult = await GetDataFromApi($"forecasts/v1/{CityCode}/historical/24");
             return CreateHistoricalForecasts(dynamicResult);
        }

        private static IEnumerable<WeatherData> CreateHistoricalForecasts(dynamic dynamicResult)
        {
            var list = new List<WeatherData>();
            foreach (var o in dynamicResult)
                list.Add(BuildHistoricalForecastFromDynamicObject(o));
            return list;
        }

        private static WeatherData BuildHistoricalForecastFromDynamicObject(dynamic o)
        {
            var builder = new WeatherDataBuilder();

            builder.SetDate((DateTime) o.DateTime)
                .SetTemperature((float) o.Temperature.Value, TemperatureUnit.Celcius)
                .SetApparentTemperature((float) o.RealFeelTemperature.Value, TemperatureUnit.Celcius)
                .SetHumidity((int) o.RelativeHumidity)
                .SetWindDirection((int) o.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Wind.Speed.Value)
                .SetChanceOfRain((int) o.PrecipitationProbability)
                .SetPrecipitationSummary((float) o.TotalLiquid.Value)
                .SetWeatherCode((int) o.WeatherIcon)
                .SetWeatherDescription((string) o.IconPhrase);
            return builder.Build();
        }

        private static dynamic ConvertRestResultToDynamicObject(IRestResponse result)
        {
            var converter = new ExpandoObjectConverter();
            dynamic d = JsonConvert.DeserializeObject<ExpandoObject[]>(result.Content, converter);

            if (d == null)
                throw new NullReferenceException("Content of the rest result was null.");
            return d;
        }

        private async Task<IRestResponse> ExecuteRestRequest(IRestRequest query)
        {
            var result = await _restClient.ExecuteAsync(query, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new HttpRequestException(result.StatusCode + ": " + result.ErrorMessage);
            return result;
        }

        private IRestRequest CreateRestRequest(string resourcePath)
        {
            var query = new RestRequest(resourcePath, DataFormat.Json);
            AddDefaultParameters(query);
            return query;
        }

        private void AddDefaultParameters(IRestRequest query)
        {
            query.AddParameter("apikey", _apiKey);
            query.AddParameter("details", true);
            query.AddParameter("language", Language);
            query.AddParameter("metric", true);
        }

        private async Task SetCityCodeIfEmpty()
        {
            if (CityCode == string.Empty)
                await ChangeCity(Latitude, Longitude);
        }

        public async Task ChangeCity(float latitude, float longitude)
        {
            var query = new RestRequest("locations/v1/cities/geoposition/search");
            query.AddParameter("apikey", _apiKey);
            query.AddParameter("q", $"{latitude}, {longitude}");
            var result = await ExecuteRestRequest(query);
            CityCode = GetCityCodeFromRestResult(result);
        }

        public async Task ChangeCity(string cityName)
        {
            var query = new RestRequest("locations/v1/cities/search");
            query.AddParameter("apikey", _apiKey);
            query.AddParameter("q", cityName);
            var result = await ExecuteRestRequest(query);
            CityCode = GetCityCodeFromRestResult(result);
        }

        private static string GetCityCodeFromRestResult(IRestResponse result)
        {
            var cityData = ConvertRestResultToDynamicObject(result);
            return cityData.Key;
        }

    }
}