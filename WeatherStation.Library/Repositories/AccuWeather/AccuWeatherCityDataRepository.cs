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

        public Task<dynamic> GetCityData(Coordinates coordinates)
        {
            if(!coordinates.IsValid())
                throw new InvalidCoordinatesException();

            _latitude = coordinates.Latitude;
            _longitude = coordinates.Longitude;
            return GetDataFromApi();
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddParameter("toplevel", true, ParameterType.QueryString);
            request.AddParameter("q", $"{_latitude}, {_longitude}", ParameterType.QueryString);
            return base.AddParametersToRequest(request);
        }

    }
}