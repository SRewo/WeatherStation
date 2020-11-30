using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Repositories
{
    public abstract class WeatherRestRepository : RestRepository, IWeatherRepository
    {
        protected string ApiKey { get; }
        public string Language { get; set; }

        protected WeatherRestRepository(IRestClient client, string resourcePath, string apiKey) : base(client, resourcePath)
        {
            ApiKey = apiKey;
        }

        public async Task<IEnumerable<WeatherData>> GetWeatherDataFromRepository()
        {
            var dynamicResult = await GetDataFromApi();
            return CreateWeatherDataListFromResult(dynamicResult);
        }

        protected virtual IEnumerable<WeatherData> CreateWeatherDataListFromResult(dynamic dynamicResult)
        {
            var list = new List<WeatherData>();

            foreach (var o in dynamicResult)
                list.Add(BuildWeatherDataFromDynamicObject(o));

            return list.AsEnumerable();
        }

        protected abstract WeatherData BuildWeatherDataFromDynamicObject(dynamic dynamicObject);
    }
}