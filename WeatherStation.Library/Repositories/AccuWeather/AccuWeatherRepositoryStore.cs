﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public class AccuWeatherRepositoryStore : IWeatherRepositoryStore
    {
        private readonly string _apiKey;
        private readonly IDateProvider _provider;
        private readonly IRestClient _restClient;
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string RepositoryName { get; set; } = "AccuWeather";
        public string Language { get; set; }
        public IWeatherRepository CurrentWeatherRepository { get; private set; }
        public IWeatherRepository DailyForecastsRepository { get; private set; }
        public IWeatherRepository HistoricalDataRepository { get; private set; }
        public IWeatherRepository HourlyForecastsRepository { get; private set; }


        public static AccuWeatherRepositoryStore FromCityCode(string key, string cityCode, IDateProvider dateProvider, IRestClient restClient)
        {
            return new AccuWeatherRepositoryStore(key, cityCode, dateProvider, restClient);
        }

        public static async Task<AccuWeatherRepositoryStore> FromCityCoordinates(string key, float cityLatitude, float cityLongitude, IDateProvider dateProvider, IRestClient restClient)
        {
            var store = new AccuWeatherRepositoryStore(key, dateProvider, restClient);
            await store.ChangeCity(cityLatitude, cityLongitude);
            return store;
        }

        private AccuWeatherRepositoryStore(string apiKey, IDateProvider provider, IRestClient restClient)
        {
            _provider = provider;
            _restClient = restClient;
            _apiKey = apiKey;
        }

        private AccuWeatherRepositoryStore(string apiKey, string cityCode, IDateProvider provider, IRestClient restClient) : this(apiKey, provider, restClient)
        {
            CityCode = cityCode;
            CreateRepositories(cityCode);
        }

        private void CreateRepositories(string cityCode)
        {
            var currentWeatherHandler = new RestRequestHandler(_restClient, $"currentconditions/v1/{cityCode}");
            var hourlyForecastHandler = new RestRequestHandler(_restClient, $"forecasts/v1/hourly/12hour/{cityCode}");
            var dailyForecastHandler = new RestRequestHandler(_restClient, $"forecasts/v1/daily/5day/{cityCode}");
            HistoricalDataRepository = null;
            CurrentWeatherRepository =
                new AccuWeatherCurrentWeatherRepository(currentWeatherHandler, _apiKey, _provider);
            HourlyForecastsRepository =
                new AccuWeatherHourlyForecastRepository(hourlyForecastHandler, _apiKey);
            DailyForecastsRepository =
                new AccuWeatherDailyForecastRepository(dailyForecastHandler, _apiKey);
        }

        public async Task ChangeCity(float latitude, float longitude)
        {
            CheckCoordinates(latitude, longitude);
            var result = await GetCityDataFromApi(latitude, longitude);
            SetCityDataProperties(result);
            CreateRepositories(CityCode);
        }

        private void SetCityDataProperties(dynamic result)
        {
            CityCode = result.Key;
            CityName = result.LocalizedName;
        }

        private async Task<dynamic> GetCityDataFromApi(float latitude, float longitude)
        {
            var handler = new RestRequestHandler(_restClient, "locations/v1/cities/geoposition/search");
            var parameters = CreateCitySearchParametersWithCoordinates(latitude, longitude);
            var result = await handler.GetDataFromApi(parameters);
            return result;
        }

        private IEnumerable<Parameter> CreateCitySearchParametersWithCoordinates(float latitude, float longitude)
        {
            var parameters = new List<Parameter>()
            {
                new Parameter("q", $"{latitude},{longitude}", ParameterType.QueryString),
                new Parameter("apikey", _apiKey, ParameterType.QueryString),
                new Parameter("details", false, ParameterType.QueryString),
                new Parameter("toplevel", true, ParameterType.QueryString),
                new Parameter("language", Language, ParameterType.QueryString)
            };
            return parameters;
        }

        private void CheckCoordinates(float latitude, float longitude)
        {
            if (IsLatitudeOutOfRange(latitude))
                throw new LatitudeOutOfRangeException("Latitude must be in range -90 to 90");

            if (IsLongitudeOutOfRange(longitude))
                throw new LongitudeOutOfRangeException("Longitude must be in range -180 to 180");
        }

        private bool IsLatitudeOutOfRange(float latitude)
        {
            return latitude < -90 || latitude > 90;
        }

        private bool IsLongitudeOutOfRange(float longitude)
        {
            return longitude < -180 || longitude > 180;
        }

        public async Task ChangeCity(string cityName)
        {
            CheckIfCityNameIsValid(cityName);
            var result = await GetCityDataFromApi(cityName);
            SetCityDataProperties(result[0]);
            CreateRepositories(CityCode);
        }

        private async Task<dynamic> GetCityDataFromApi(string cityName)
        {
            var parameters = CreateCitySearchWithNameParameters(cityName);
            var handler = new RestRequestHandler(_restClient, "locations/v1/cities/search");
            var result = await handler.GetDataFromApi(parameters);
            CheckIfResultContainsOnlyOneCity(result);
            return result;
        }

        private static void CheckIfResultContainsOnlyOneCity(dynamic result)
        {
            var list = (ExpandoObject[]) result;

            if (list.Length != 1)
                throw new MultipleCitiesResultException("City search with name returned multiple cites. Provide additional information after ',' ex. CityName,Country");
        }

        private List<Parameter> CreateCitySearchWithNameParameters(string cityName)
        {
            var parameters = new List<Parameter>()
            {
                new Parameter("q", cityName, ParameterType.QueryString),
                new Parameter("details", false, ParameterType.QueryString),
                new Parameter("apikey", _apiKey, ParameterType.QueryString),
                new Parameter("language", Language, ParameterType.QueryString)
            };
            return parameters;
        }

        private void CheckIfCityNameIsValid(string cityName)
        {
            if (string.IsNullOrEmpty(cityName))
                throw new InvalidCityNameException("City name cannot be empty");

            if (ContainsSpecialCharacters(cityName))
                throw new InvalidCityNameException("City name cannot contain special characters");
        }

        private bool ContainsSpecialCharacters(string cityName)
        {
            var specialCharacters = "!@#$%^&*()_-=+/<>?\\|";
            foreach (var specialCharacter in specialCharacters)
                if (cityName.Contains(specialCharacter))
                    return true;

            return false;
        }

        public async Task ChangeLanguage(string languageCode)
        {
        }
    }

    public class MultipleCitiesResultException : Exception
    {
        public MultipleCitiesResultException(string message) : base(message)
        {

        }
    }

    public class InvalidCityNameException : Exception
    {
        public InvalidCityNameException(string message) : base(message)
        {

        }

    }

    public class LatitudeOutOfRangeException : Exception
    {
        public LatitudeOutOfRangeException(string message) : base(message)
        {
        }
    }

    public class LongitudeOutOfRangeException : Exception
    {
        public LongitudeOutOfRangeException() : base()
        {
        }

        public LongitudeOutOfRangeException(string message) : base(message)
        {
        }

        public LongitudeOutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}