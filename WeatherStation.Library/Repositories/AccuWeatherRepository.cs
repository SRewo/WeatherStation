using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories
{
    public class AccuWeatherRepository : IWeatherRepository ,IContainsDailyForecast, IContainsHourlyForecast, IContainsHistoricalData
    {
        public static AccuWeatherRepository CreateInstanceWithCityCode(string key, string cityCode, IDateProvider provider, IRestClient client)
        {
            return new AccuWeatherRepository(key, cityCode, provider, client);
        }

        public static AccuWeatherRepository CreateInstanceWithCoordinates(string key, float latitude, float longitude, IDateProvider provider, IRestClient client)
        {
            return new AccuWeatherRepository(key, latitude, longitude, provider, client);
        }

        private string _key;
        private IDateProvider _dateProvider;
        private IRestClient _client;

        public string CityCode { get; set; }
        public string Language { get; set; }

        private AccuWeatherRepository(string key, float latitude, float longitude, IDateProvider provider, IRestClient client)
        {
            _key = key;
            Latitude = latitude;
            Longitude = longitude;
            _dateProvider = provider;
            _client = client;
        }

        private AccuWeatherRepository(string key, string cityCode, IDateProvider provider, IRestClient client)
        {
            _key = key;
            CityCode = cityCode;
            _dateProvider = provider;
            _client = client;
        }

        public string RepositoryName { get; set; } = "AccuWeather";
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public async Task<WeatherData> GetCurrentWeather()
        {

            if (CityCode == string.Empty)
                CityCode = await GetCityCode();

            var query = new RestRequest($"currentconditions/v1/{CityCode}", DataFormat.Json);
            query.AddParameter("apikey", _key);
            query.AddParameter("details", true);
            query.AddParameter("language", Language);
            var result = await _client.ExecuteAsync(query, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new HttpRequestException(result.StatusCode.ToString());

            var converter = new ExpandoObjectConverter();
            dynamic d = JsonConvert.DeserializeObject<ExpandoObject[]>(result.Content, converter);

            if(d == null)
                throw new NullReferenceException();

            var o = d[0];
            var builder = new WeatherDataBuilder();
            builder.SetTemperature((float) o.Temperature.Metric.Value, TemperatureUnit.Celcius)
                .SetApparentTemperature((float) o.RealFeelTemperature.Metric.Value, TemperatureUnit.Celcius)
                .SetHumidity((int) o.RelativeHumidity)
                .SetWindDirection((int) o.Wind.Direction.Degrees)
                .SetWindSpeed((float) o.Wind.Speed.Metric.Value)
                .SetPressure((int) o.Pressure.Metric.Value)
                .SetPrecipitationSummary((float) o.PrecipitationSummary.Precipitation.Metric.Value)
                .SetWeatherCode((int) o.WeatherIcon)
                .SetWeatherDescription((string) o.WeatherText);

            return builder.Build();
        }

        public int DailyForecastHorizon { get; } = 5;
        public async Task<IEnumerable<WeatherData>> GetDailyForecast()
        {
            if (CityCode == string.Empty)
                CityCode = await GetCityCode();

            var query = new RestRequest($"forecasts/v1/daily/5day/{CityCode}", DataFormat.Json);
            query.AddParameter("apikey", _key);
            query.AddParameter("details", true);
            query.AddParameter("language", Language);
            query.AddParameter("metric", true);
            var result = await _client.ExecuteAsync(query, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new HttpRequestException(result.StatusCode.ToString());

            var converter = new ExpandoObjectConverter();
            dynamic d = JsonConvert.DeserializeObject<ExpandoObject>(result.Content, converter);

            if(d == null)
                throw new NullReferenceException();
             
            var list = new List<WeatherData>();
    
            foreach (var o in d.DailyForecasts)
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

                list.Add(builder.Build());
            }

            return list;

        }

        public int HourlyForecastHorizon { get; } = 12;
        public async Task<IEnumerable<WeatherData>> GetHourlyForecast()
        {
             if (CityCode == string.Empty)
                CityCode = await GetCityCode();

             var query = new RestRequest($"forecasts/v1/hourly/12hour/{CityCode}", DataFormat.Json);
             query.AddParameter("apikey", _key);
             query.AddParameter("details", true);
             query.AddParameter("language", Language);
             query.AddParameter("metric", true);
             var result = await _client.ExecuteAsync(query, CancellationToken.None);

             if (!result.IsSuccessful)
                 throw new HttpRequestException(result.StatusCode.ToString());

             var converter = new ExpandoObjectConverter();
             dynamic d = JsonConvert.DeserializeObject<ExpandoObject[]>(result.Content, converter);

             if(d == null)
                 throw new NullReferenceException("Provided json string is empty");
             
             var list = new List<WeatherData>();

             foreach (var o in d)
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

                 list.Add(builder.Build());
             }

             return list;
        }

        public int MaxHistoricalHorizon { get; } = 24;
        public TimeSpan DifferenceBetweenMeasurements { get; } = TimeSpan.FromHours(1);
        public async Task<IEnumerable<WeatherData>> GetHistoricalData(DateTime @from)
        {
             if (CityCode == string.Empty)
                CityCode = await GetCityCode();

             var query = new RestRequest($"forecasts/v1/{CityCode}/historical/24", DataFormat.Json);
             query.AddParameter("apikey", _key);
             query.AddParameter("details", true);
             query.AddParameter("language", Language);
             query.AddParameter("metric", true);
             var result = await _client.ExecuteAsync(query, CancellationToken.None);

             if (!result.IsSuccessful)
                 throw new HttpRequestException(result.StatusCode.ToString());

             var converter = new ExpandoObjectConverter();
             dynamic d = JsonConvert.DeserializeObject<ExpandoObject[]>(result.Content, converter);

             if(d == null)
                 throw new NullReferenceException();
             
             var list = new List<WeatherData>();

             foreach (var o in d)
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

                 list.Add(builder.Build());
             }

             var filteredList = list.Where(x => x.Date > from);

             return filteredList;
        }

        public async Task<IEnumerable<WeatherData>> GetHistoricalData(DateTime @from, DateTime to)
        {
            if (CityCode == string.Empty)
                CityCode = await GetCityCode();

            var query = new RestRequest($"forecasts/v1/{CityCode}/historical/24", DataFormat.Json);
            query.AddParameter("apikey", _key);
            query.AddParameter("details", true);
            query.AddParameter("language", Language);
            query.AddParameter("metric", true);
            var result = await _client.ExecuteAsync(query, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new HttpRequestException(result.StatusCode.ToString());

            var converter = new ExpandoObjectConverter();
            dynamic d = JsonConvert.DeserializeObject<ExpandoObject[]>(result.Content, converter);

            if(d == null)
                throw new NullReferenceException();
             
            var list = new List<WeatherData>();

            foreach (var o in d)
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

                list.Add(builder.Build());
            }

            var filteredList = list.Where(x => x.Date > from && x.Date < to);

            return filteredList;
        }

        public async Task<string> GetCityCode()
        {
            var query = new RestRequest("locations/v1/cities/geoposition/search");
            query.AddParameter("apikey", _key);
            query.AddParameter("q", $"{Latitude}, {Longitude}");

            var result = await _client.ExecuteAsync(query, CancellationToken.None);

            if(!result.IsSuccessful)
                throw new HttpRequestException(result.StatusCode + " " + result.ErrorMessage);

            var converter = new ExpandoObjectConverter();
            dynamic d = JsonConvert.DeserializeObject<ExpandoObject>(result.Content, converter);

            return d.Key;
        }

        public IContainsDailyForecast DailyRepository => this;
        public IContainsHistoricalData HistoricalRepository => this;
        public IContainsHourlyForecast HourlyRepository => this;
    }
}