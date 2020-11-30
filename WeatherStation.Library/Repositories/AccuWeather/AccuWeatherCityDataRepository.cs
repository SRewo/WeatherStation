using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

namespace WeatherStation.Library.Repositories.AccuWeather
{
    public abstract class AccuWeatherCityDataRepository : RestRepository
    {
        private readonly string _apiKey;
        public string Language { get; set; }
        public AccuWeatherCityDataRepository(IRestClient restClient, string resourcePath, string apiKey, string language) : base(restClient, resourcePath)
        {
            Language = language;
            _apiKey = apiKey;
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("apikey", _apiKey, ParameterType.QueryString);
            request.AddParameter("details", false, ParameterType.QueryString);
            request.AddParameter("language", Language, ParameterType.QueryString);
            return Task.CompletedTask;
        }
    }

    public class AccuWeatherCityDataFromGeolocation : AccuWeatherCityDataRepository
    {
        private float _latitude;
        private float _longitude;

        public AccuWeatherCityDataFromGeolocation(IRestClient restClient, string apiKey, string language) : base(restClient, "locations/v1/cities/geoposition/search", apiKey, language)
        {

        }

        public Task<dynamic> GetCityData(float latitude, float longitude)
        {
            CheckCoordinates(latitude, longitude);
            _latitude = latitude;
            _longitude = longitude;
            return GetDataFromApi();
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

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("toplevel", true, ParameterType.QueryString);
            request.AddParameter("q", $"{_latitude}, {_longitude}", ParameterType.QueryString);
            return base.AddParametersToRequest(request);
        }

        public class LatitudeOutOfRangeException : Exception
        {
            public LatitudeOutOfRangeException(string message) : base(message)
            {
            }

            public LatitudeOutOfRangeException() : base()
            {
            }

            public LatitudeOutOfRangeException(string message, Exception innerException) : base(message, innerException)
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

    public class AccuWeatherCityDataFromCityName : AccuWeatherCityDataRepository
    {
        private string _cityName;

        public AccuWeatherCityDataFromCityName(IRestClient restClient, string apiKey, string language) : base(restClient, "locations/v1/cities/search", apiKey, language)
        {
        }

        public async Task<dynamic> GetCityData(string cityName)
        {
            CheckIfCityNameIsValid(cityName);
            _cityName = cityName;
            var result = await GetDataFromApi();
            CheckIfResultContainsOnlyOneCity(result);
            return result;
        }

        private static void CheckIfResultContainsOnlyOneCity(dynamic result)
        {
            var list = (ExpandoObject[]) result;

            if (list.Count() > 1)
                throw new MultipleCitiesResultException("City search with name returned multiple cites. Provide additional information after ',' ex. CityName,Country or CityName,StateName");
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("q", _cityName, ParameterType.QueryString);
            return base.AddParametersToRequest(request);
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
            var specialCharacters = "!@#$%^&*()_=+<>?|";
            foreach (var specialCharacter in specialCharacters)
                if (cityName.Contains(specialCharacter))
                    return true;

            return false;
        }

        public class MultipleCitiesResultException : Exception
        {
            public MultipleCitiesResultException(string message) : base(message)
            {

            }

            public MultipleCitiesResultException() : base()
            {
            }

            public MultipleCitiesResultException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        public class InvalidCityNameException : Exception
        {
            public InvalidCityNameException(string message) : base(message)
            {

            }

            public InvalidCityNameException() : base()
            {
            }

            public InvalidCityNameException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

    }
}