using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using WeatherStation.Services.Services;
using Xunit;

namespace WeatherStation.Services.Tests
{
    public class WeatherServiceTests
    {
        private IMapper _mapper;

        public WeatherServiceTests()
        {
            var profile = new ServiceMapperProfile();
            var config = new MapperConfiguration(x =>
                x.AddProfile(profile));
            _mapper = new Mapper(config);
        }

        [Fact]
        public async Task GetRepositoryList_ValidCall_ReturnsProperObjectType()
        {
            var logger = new Mock<ILogger<WeatherService>>();
            var service = new WeatherService(logger.Object, null, _mapper);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.IsAssignableFrom<IEnumerable<Repositories>>(result.ListOfRepositories);
        }

        [Fact]
        public async Task GetRepositoryList_ValidCall_IsNotEmpty()
        {
            var logger = new Mock<ILogger<WeatherService>>();
            var service = new WeatherService(logger.Object,null, _mapper);

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
            var service = new WeatherService(logger.Object, null, _mapper);

            var result = await service.GetRepositoryList(new ListRequest(), null);

            Assert.Contains(repo, result.ListOfRepositories);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperRepositoryName()
        {
            var service = await PrepareWeatherService();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.Equal("Testing", result.RepositoryName);
        }

        private async Task<WeatherService> PrepareWeatherService()
        {

            var logger = new Mock<ILogger<WeatherService>>();
            var repositories = await PrepareRepositoryForTesting();
            return new WeatherService(logger.Object, repositories, _mapper);
        }

        private async Task<Dictionary<Repositories, IWeatherRepositoryStore>> PrepareRepositoryForTesting()
        {

            var repository = new Mock<IWeatherRepositoryStore>();
            repository.Setup(x => x.RepositoryName).Returns("Testing");
            repository.Setup(x => x.ContainsHistoricalData).Returns(false);
            repository.Setup(x => x.ContainsDailyForecasts).Returns(true);
            repository.Setup(x => x.ContainsHourlyForecasts).Returns(true);
            repository.Setup(x => x.CurrentWeatherRepository).Returns(await PrepareCurrentWeatherRepository());
            var dictionary = new Dictionary<Repositories, IWeatherRepositoryStore>
            {
                {Repositories.Accuweather, repository.Object}
            };
            return dictionary;
        }

        private Task<IWeatherRepository> PrepareCurrentWeatherRepository()
        {
            var repository = new Mock<IWeatherRepository>();
            var weatherData = new WeatherData()
            {
                ChanceOfRain = 10,
                Date = new DateTime(2020, 01, 01, 10, 10, 10),
                Temperature = new CelsiusTemperature(12),
                Humidity = 10,
                TemperatureApparent = new CelsiusTemperature(13.1f),
                WeatherDescription = ""
            };
            repository.Setup(x => x.GetWeatherDataFromRepository()).ReturnsAsync(new List<WeatherData> {weatherData});
            return Task.FromResult(repository.Object);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperDailyForecastFlag()
        {
            var service = await PrepareWeatherService();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.True(result.ContainsDailyForecasts);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperHourlyForecastFlag()
        {
            var service = await PrepareWeatherService();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.True(result.ContainsHourlyForecasts);
        }

        [Fact]
        public async Task GetRepositoryInfo_ValidCall_SetsProperHistoricalDataFlag()
        {
            var service = await PrepareWeatherService();
            var request = new InfoRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetRepositoryInfo(request, null);

            Assert.False(result.ContainsHistoricalData);
        }

        [Fact]
        public async Task GetRepositoryInfo_MissingRepository_ThrowsRpcException()
        {
            var service = await PrepareWeatherService();
            var request = new InfoRequest() {Repository = Repositories.Weatherbit};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetRepositoryInfo(request, null));
        }

        [Fact]
        public async Task GetCurrentWeather_ReturnsProperObjectType()
        {
            var service = await PrepareWeatherService();
            var request = new CurrentWeatherRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetCurrentWeather(request, null);

            Assert.IsType<CurrentWeatherReply>(result);
        }

        [Fact]
        public async Task GetCurrentWeather_SetsProperTemperatureEnum()
        {
            var service = await PrepareWeatherService();
            var request = new CurrentWeatherRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetCurrentWeather(request, null);

            Assert.Equal(12, result.Weather.Temperature.Value);
        }

        [Fact]
        public async Task GetCurrentWeather_SetsProperTemperatureValue()
        {
            var service = await PrepareWeatherService();
            var request = new CurrentWeatherRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetCurrentWeather(request, null);

            Assert.Equal(TemperatureScale.Celsius, result.Weather.Temperature.Scale);
        }

        [Fact]
        public async Task GetCurrentWeather_SetsProperDateTimestamp()
        {
            var service = await PrepareWeatherService();
            var request = new CurrentWeatherRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetCurrentWeather(request, null);

            Assert.NotEqual(new Timestamp(), result.Weather.Date);
        }

        [Fact]
        public async Task GetCurrentWeather_MissingRepository_ThrowsRpcException()
        {
            var service = await PrepareWeatherService();
            var request = new CurrentWeatherRequest() {Repository = Repositories.Weatherbit};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetCurrentWeather(request, null));
        }
    }
}
