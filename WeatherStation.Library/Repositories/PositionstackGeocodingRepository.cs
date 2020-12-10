using RestSharp;
using System;
using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories
{
    public class PositionstackGeocodingRepository : RestRepository, IGeocodingRepository
    {
        private string _locationData;
        private string _apiKey;
        public PositionstackGeocodingRepository(IRestClient restClient, string apiKey) : base(restClient, "forward")
        {
            _apiKey = apiKey;
        }

        public async Task<(double, double)> GetLocationCoordinates(string locationData)
        {
            if(string.IsNullOrWhiteSpace(locationData))
                throw new NoLocationDataException("No location data was provided for coordinates search.");

            _locationData = locationData;
            var data = await GetDataFromApi();
            var result = data.data[0];
            return (result.latitude, result.longitude);
        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            request.AddQueryParameter("access_key", _apiKey);
            request.AddQueryParameter("query", _locationData);
            request.AddQueryParameter("limit", "1");
            return Task.FromResult(request);
        }
    }

    public class NoLocationDataException : Exception
    {
        public NoLocationDataException(string message) : base(message) { }
    }
}
