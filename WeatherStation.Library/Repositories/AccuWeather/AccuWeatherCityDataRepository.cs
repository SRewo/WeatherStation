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
        private double _latitude;
        private double _longitude;

        public AccuWeatherCityDataFromGeolocation(IRestClient restClient, string apiKey, string language) : base(restClient, "locations/v1/cities/geoposition/search", apiKey, language)
        {

        }

        public Task<dynamic> GetCityData(double latitude, double longitude)
        {
            CheckCoordinates(latitude, longitude);
            _latitude = latitude;
            _longitude = longitude;
            return GetDataFromApi();
        }

        private void CheckCoordinates(double latitude, double longitude)
        {
            if (IsLatitudeOutOfRange(latitude))
                throw new LatitudeOutOfRangeException("Latitude must be in range -90 to 90");

            if (IsLongitudeOutOfRange(longitude))
                throw new LongitudeOutOfRangeException("Longitude must be in range -180 to 180");
        }

        private bool IsLatitudeOutOfRange(double latitude)
        {
            return latitude < -90 || latitude > 90;
        }

        private bool IsLongitudeOutOfRange(double longitude)
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
}