using System;
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
        private IEnumerable<WeatherData> _weatherData = new List<WeatherData>();
        protected IDateProvider DateProvider;
        private DateTime _dataDownloadTime = DateTime.MinValue;
        public string Language { get; set; }

        protected WeatherRestRepository(IRestClient client, string resourcePath, string apiKey, IDateProvider dateProvider) : base(client, resourcePath)
        {
            ApiKey = apiKey;
            DateProvider = dateProvider;
        }

        public async Task<IEnumerable<WeatherData>> GetWeatherDataFromRepository()
        {
            if(IsStoredDataOlderThan30Minutes()) 
                await DownloadData();

            return _weatherData;
        }

        private bool IsStoredDataOlderThan30Minutes()
        {
            return (DateProvider.GetActualDateTime() - _dataDownloadTime) > TimeSpan.FromMinutes(30);
        }

        private async Task DownloadData()
        {
            var dynamicResult = await GetDataFromApi();
            _weatherData = CreateWeatherDataListFromResult(dynamicResult);
            _dataDownloadTime = DateProvider.GetActualDateTime();
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