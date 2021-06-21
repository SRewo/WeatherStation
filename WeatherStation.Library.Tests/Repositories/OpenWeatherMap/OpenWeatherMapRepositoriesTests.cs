using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.Core.Internal;
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
            var dateProviderMock = CommonMethods.CreateDateProviderMock();
            var clientMock = await
                CommonMethods.CreateRestClientMock("Repositories/OpenWeatherMap/TestResponses/TestResponse.json");
            var repository =
                new OpenWeatherMapCurrentWeatherRepository(clientMock.Object, "", "", dateProviderMock.Object);
            return repository;
        }
    }
}