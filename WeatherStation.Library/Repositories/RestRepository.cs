using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;

namespace WeatherStation.Library.Repositories
{
    public abstract class RestRepository
    {
        private readonly IRestClient _restClient;
        private readonly string _resourcePath;

        protected RestRepository(IRestClient restClient, string resourcePath)
        {
            _restClient = restClient;
            _resourcePath = resourcePath;
        }

        protected virtual async Task<dynamic> GetDataFromApi()
        {
            var query = await CreateRestRequest();
            var result = await ExecuteRestRequest(query);
            return ConvertRestResultToDynamicObject(result);
        }
        private static dynamic ConvertRestResultToDynamicObject(IRestResponse result)
        {
            var d = DeserializeObjectToDynamicObject(result.Content);

            if (d == null)
                throw new NullReferenceException("Content of the rest result was null.");
            return d;
        }

        private static dynamic DeserializeObjectToDynamicObject(string stringToDeserialize)
        {
            var converter = new ExpandoObjectConverter();
            dynamic d;
            try
            {
                d = JsonConvert.DeserializeObject<ExpandoObject[]>(stringToDeserialize, converter);
            }
            catch (JsonSerializationException)
            {
                d = JsonConvert.DeserializeObject<ExpandoObject>(stringToDeserialize, converter);
            }

            return d;
        }

        private async Task<IRestResponse> ExecuteRestRequest(IRestRequest query)
        {
            var result = await _restClient.ExecuteAsync(query, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new HttpRequestException(result.StatusCode + ": " + result.ErrorMessage);
            return result;
        }

        private async Task<IRestRequest> CreateRestRequest()
        {
            var request = new RestRequest(_resourcePath, DataFormat.Json);
            await AddParametersToRequest(request);
            return request;
        }

        protected abstract Task AddParametersToRequest(IRestRequest request);

    }
}