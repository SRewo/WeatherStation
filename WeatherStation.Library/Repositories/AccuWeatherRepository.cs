using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class AccuWeatherRepository : IContainsDailyForecast, IContainsHourlyForecast, IContainsHistoricalData
    {
        private string _address = "http://dataservice.accuweather.com";
        private string _key;

        public string CityCode { get; set; }
        public string Language { get; set; }

        public AccuWeatherRepository(string key, float latitude, float longitude)
        {
            _key = key;
            Latitude = latitude;
            Longitude = longitude;
        }

        public AccuWeatherRepository(string key, string cityCode)
        {
            _key = key;
            CityCode = cityCode;
        }

        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public async Task<WeatherData> GetCurrentWeather()
        {

            if (CityCode == string.Empty)
                await GetCityCode();

            var client = new RestClient(_address);

            var query = new RestRequest($"currentconditions/v1/{CityCode}", DataFormat.Json);
            query.AddParameter("apikey", _key);
            query.AddParameter("details", true);
            query.AddParameter("language", Language);
            var result = await client.ExecuteAsync(query, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new InvalidOperationException(result.StatusCode.ToString());

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
                await GetCityCode();

            var client = new RestClient(_address);

            var query = new RestRequest($"forecasts/v1/daily/5day/{CityCode}", DataFormat.Json);
            query.AddParameter("apikey", _key);
            query.AddParameter("details", true);
            query.AddParameter("language", Language);
            query.AddParameter("metric", true);
            var result = await client.ExecuteAsync(query, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new InvalidOperationException(result.StatusCode.ToString());

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
                await GetCityCode();

             var client = new RestClient(_address);

             var query = new RestRequest($"forecasts/v1/hourly/12hour/{CityCode}", DataFormat.Json);
             query.AddParameter("apikey", _key);
             query.AddParameter("details", true);
             query.AddParameter("language", Language);
             query.AddParameter("metric", true);
             var result = await client.ExecuteAsync(query, CancellationToken.None);

             if (!result.IsSuccessful)
                 throw new InvalidOperationException(result.StatusCode.ToString());

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

             return list;
        }

        public int MaxHistoricalHorizon { get; } = 24;
        public TimeSpan DifferenceBetweenMeasurements { get; } = TimeSpan.FromHours(1);
        public Task<IEnumerable<WeatherData>> GetHistoricalData(DateTime @from)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WeatherData>> GetHistoricalData(DateTime @from, DateTime to)
        {
            throw new NotImplementedException();
        }

        private Task GetCityCode()
        {
            throw new NotImplementedException();
        }

    }
}