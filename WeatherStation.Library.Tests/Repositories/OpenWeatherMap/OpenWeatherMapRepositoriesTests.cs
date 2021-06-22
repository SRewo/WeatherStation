using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Moq;
using RestSharp;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories.OpenWeatherMap;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories.OpenWeatherMap
{
    public class OpenWeatherMapRepositoriesTests
    {
        [Fact]
        public async Task CurrentWeatherGet_ProperExecution_ReturnsSingleObject()
        {
            var repository = await CreateCurrentWeatherRepository();

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.Single(weather);
        }

        private async Task<OpenWeatherMapCurrentWeatherRepository> CreateCurrentWeatherRepository()
        {
            var mocks = await CreateMocks();
            var repository =
                new OpenWeatherMapCurrentWeatherRepository(mocks.restClient.Object, "", "", mocks.dateProvider.Object);
            return repository;
        }

        private async Task<(Mock<IDateProvider> dateProvider, Mock<IRestClient> restClient)> CreateMocks()
        {
            
            var dateProviderMock = CommonMethods.CreateDateProviderMock();
            var clientMock = await
                CommonMethods.CreateRestClientMock("Repositories/OpenWeatherMap/TestResponses/TestResponse.json");
            return (dateProviderMock, clientMock);
        }

        [Fact]
        public async Task DailyForecastsGetData_ProperExecution_ReturnsValidCollection()
        {
            var repository = await CreateDailyForecastsRepository();

            var weatherData = await repository.GetWeatherDataFromRepository();

            Assert.True(weatherData.Count() > 1);
        }

        private async Task<OpenWeatherMapDailyForecastsRepository> CreateDailyForecastsRepository()
        {
            var mocks = await CreateMocks();
            var repository =
                new OpenWeatherMapDailyForecastsRepository(mocks.restClient.Object, "", "", mocks.dateProvider.Object);
            return repository;
        }

        [Fact]
        public async Task HourlyForecastsGetData_ValidCall_ReturnsCollectionWithMultipleForecastsData()
        {
            var repository = await CreateHourlyForecastRepository();

            var forecasts = await repository.GetWeatherDataFromRepository();

            Assert.True(forecasts.Count() > 1);
        }

        private async Task<OpenWeatherMapHourlyForecastsRepository> CreateHourlyForecastRepository()
        {
            var mocks = await CreateMocks();
            var repository =
                new OpenWeatherMapHourlyForecastsRepository(mocks.restClient.Object, "", "", mocks.dateProvider.Object);
            return repository;
        }
    }
}