using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherStation.Library.Interfaces;
using WeatherStation.Services.Services;
using Xunit;

namespace WeatherStation.Services.Tests
{
    public class WeatherServiceTests
    {
        [Fact]
        public async Task GetRepositoryList_ValidCall_ReturnsProperObjectType()
        {
            var logger = new Mock<ILogger<WeatherService>>();
            var service = new WeatherService(logger.Object, null);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.IsAssignableFrom<IEnumerable<Repositories>>(result.ListOfRepositories);
        }

        [Fact]
        public async Task GetRepositoryList_ValidCall_IsNotEmpty()
        {
            var logger = new Mock<ILogger<WeatherService>>();
            var service = new WeatherService(logger.Object,null);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.NotEmpty(result.ListOfRepositories);
        }

        [Theory]
        [InlineData(Repositories.Weatherbit)]
        [InlineData(Repositories.Accuweather)]
        [InlineData(Repositories.Openweathermap)]
        public async Task GetRepositoryList_ValidCall_ContainsRepository(Repositories repo)
        {
            var logger = new Mock<ILogger<WeatherService>>();
            var service = new WeatherService(logger.Object, null);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.Contains(repo, result.ListOfRepositories);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperRepositoryName()
        {
            var service = await PrepareWeatherServiceForRepositoryInfoTesting();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.Equal("Testing", result.RepositoryName);
        }

        private async Task<WeatherService> PrepareWeatherServiceForRepositoryInfoTesting()
        {

            var logger = new Mock<ILogger<WeatherService>>();
            var repositories = await PrepareRepositoryForTesting();
            return new WeatherService(logger.Object, repositories);
        }

        private Task<Dictionary<Repositories, IWeatherRepositoryStore>> PrepareRepositoryForTesting()
        {

            var repository = new Mock<IWeatherRepositoryStore>();
            repository.Setup(x => x.RepositoryName).Returns("Testing");
            repository.Setup(x => x.ContainsHistoricalData).Returns(false);
            repository.Setup(x => x.ContainsDailyForecasts).Returns(true);
            repository.Setup(x => x.ContainsHourlyForecasts).Returns(true);
            var dictionary = new Dictionary<Repositories, IWeatherRepositoryStore>
            {
                {Repositories.Accuweather, repository.Object}
            };
            return Task.FromResult(dictionary);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperDailyForecastFlag()
        {
            var service = await PrepareWeatherServiceForRepositoryInfoTesting();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.True(result.ContainsDailyForecasts);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperHourlyForecastFlag()
        {
            var service = await PrepareWeatherServiceForRepositoryInfoTesting();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.True(result.ContainsHourlyForecasts);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperHistoricalDataFlag()
        {
            var service = await PrepareWeatherServiceForRepositoryInfoTesting();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.False(result.ContainsHistoricalData);
        }

        [Fact]
        public async Task GetRepositoryInfo_MissingRepository_ThrowsRpcException()
        {
            var service = await PrepareWeatherServiceForRepositoryInfoTesting();
            var request = new InfoRequest() {Repository = Repositories.Weatherbit};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetRepositoryInfo(request, null));
        }
    }
}
