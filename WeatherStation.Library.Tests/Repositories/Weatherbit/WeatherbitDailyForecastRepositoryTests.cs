﻿using Moq;
using RestSharp;
using System.Linq;
using System.Threading.Tasks;
using WeatherStation.Library.Repositories.Weatherbit;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories.Weatherbit
{
    public class WeatherbitDailyForecastRepositoryTests
    {
        private string _testResponseLocation = "Repositories/Weatherbit/TestResponses/dailyWeather.json";

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_ReturnsNotEmptyList()
        {
            var classes = await CreateTestClasses();

            var items = await classes.Item1.GetWeatherDataFromRepository();

            Assert.NotEmpty(items);
        }

        private async Task<(WeatherbitDailyForecastRepository, Mock<IRestClient>)> CreateTestClasses()
        {
            var dateProviderMock = CommonMethods.CreateDateProviderMock();
            var client = await CommonMethods.CreateRestClientMock(_testResponseLocation);
            var repository = new WeatherbitDailyForecastRepository(client.Object,"", new Coordinates(0,0), dateProviderMock.Object);
            repository.Language = "pl-PL";
            return (repository, client);
        }

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_ReturnsForecastFor16Days()
        {
            var classes = await CreateTestClasses();

            var items = await classes.Item1.GetWeatherDataFromRepository();

            Assert.Equal(16, items.Count());
        }

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_DoesNotContaionDefaultWeatherDataObjects()
        {
            var classes = await CreateTestClasses();

            var items = await classes.Item1.GetWeatherDataFromRepository();

            Assert.DoesNotContain(new WeatherData(), items);
        }
    }
}
