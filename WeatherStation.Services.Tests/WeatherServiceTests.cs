using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherStation.Services.Services;
using Xunit;

namespace WeatherStation.Services.Tests
{
    public class WeatherServiceTests
    {
        [Fact]
        public async Task GetRepositoryList_ValidCall_ReturnsProperObjectType()
        {
            var logger = new Mock<ILogger<Services.WeatherService>>();
            var service = new Services.WeatherService(logger.Object);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.IsAssignableFrom<IEnumerable<Repositories>>(result.ListOfRepositories);
        }

        [Fact]
        public async Task GetRepositoryList_ValidCall_IsNotEmpty()
        {
            var logger = new Mock<ILogger<Services.WeatherService>>();
            var service = new Services.WeatherService(logger.Object);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.NotEmpty(result.ListOfRepositories);
        }

        [Theory]
        [InlineData(Repositories.Weatherbit)]
        [InlineData(Repositories.Accuweather)]
        [InlineData(Repositories.Openweathermap)]
        public async Task GetRepositoryList_ValidCall_ContainsRepository(Repositories repo)
        {
            var logger = new Mock<ILogger<Services.WeatherService>>();
            var service = new Services.WeatherService(logger.Object);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.Contains(repo, result.ListOfRepositories);
        }
    }
}
