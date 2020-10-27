using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories
{
    public abstract class WeatherRestRepository : IWeatherRepository
    {
        protected string ApiKey { get; }
        public string Language { get; set; }
        private readonly RestRequestHandler _handler;

        protected WeatherRestRepository(RestRequestHandler handler, string apiKey)
        {
            ApiKey = apiKey;
            _handler = handler;
        }

        public async Task<IEnumerable<WeatherData>> GetWeatherDataFromRepository()
        {
            var parameters = CreateRequestParameters();
            var dynamicResult = await _handler.GetDataFromApi(parameters);
            return CreateWeatherDataListFromResult(dynamicResult);
        }

        protected virtual IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            var list = new List<WeatherData>();

            foreach (var o in dynamicResult)
                list.Add(BuildWeatherDataFromDynamicObject(o));

            return list.AsEnumerable();
        }

        protected abstract IEnumerable<Parameter> CreateRequestParameters();

        protected abstract WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject);
    }
}