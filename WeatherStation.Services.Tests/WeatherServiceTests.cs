using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
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
            var repositoryWithHourlyForecasts = await TestMockFactory.CreateRepositoryStoreMock(false, true);
            var repositoryWithDailyForecasts = await TestMockFactory.CreateRepositoryStoreMock(true, false);

            var dictionary = new Dictionary<Repositories, IWeatherRepositoryStore>
            {
                {Repositories.Accuweather, repositoryWithDailyForecasts.Object},
                {Repositories.Openweathermap, repositoryWithHourlyForecasts.Object }
            };
            return dictionary;
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

            Assert.False(result.ContainsHourlyForecasts);
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
            var request = new WeatherRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetCurrentWeather(request, null);

            Assert.IsType<CurrentWeatherReply>(result);
        }

        [Fact]
        public async Task GetCurrentWeather_SetsProperDateTimestamp()
        {
            var service = await PrepareWeatherService();
            var request = new WeatherRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetCurrentWeather(request, null);

            Assert.NotEqual(new Timestamp(), result.Weather.Date);
        }

        [Fact]
        public async Task GetCurrentWeather_MissingRepository_ThrowsRpcException()
        {
            var service = await PrepareWeatherService();
            var request = new WeatherRequest() {Repository = Repositories.Weatherbit};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetCurrentWeather(request, null));
        }

        [Fact]
        public async Task GetCurrentWeather_ThrowsAnyException_ThrowsRpcException()
        {
            var exception = new Exception("123");
            var service = await CreateServiceThatThrowsException(exception);
            var request = new WeatherRequest() {Repository = Repositories.Accuweather};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetCurrentWeather(request, null));
        }

        [Fact]
        public async Task GetDailyForecasts_ValidCall_ReturnsProperNumberOfItems()
        {
            var service = await PrepareWeatherService();
            var request = new WeatherRequest() {Repository = Repositories.Accuweather};

            var result = await service.GetDailyForecasts(request, null);

            Assert.Equal(10, result.Forecasts.Count());
        }

        [Fact]
        public async Task GetDailyForecast_RepositoryStoreDoesNotHaveRepository_ThrowsRpcException()
        {
            var service = await PrepareWeatherService();
            var request = new WeatherRequest() {Repository = Repositories.Openweathermap};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetDailyForecasts(request, null));
        }

        [Fact]
        public async Task GetDailyForecast_DictionaryDoesNotHaveRepositoryStore_ThrowsRpcException()
        {
            var service = await PrepareWeatherService();
            var request = new WeatherRequest() {Repository = Repositories.Weatherbit};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetDailyForecasts(request, null));
        }

        [Fact]
        public async Task GetDailyForecast_GetDataFromRepositoryThrowsHtmlException_ThrowsRpcException()
        {
            var exception = new HttpRequestException("Bad request");
            var service = await CreateServiceThatThrowsException(exception);
            var request = new WeatherRequest() {Repository = Repositories.Accuweather};

            await Assert.ThrowsAnyAsync<RpcException>(() => service.GetDailyForecasts(request, null));
        }

        private async Task<WeatherService> CreateServiceThatThrowsException(Exception ex)
        {
            var repositoryStoreMock = await TestMockFactory.CreateRepositoryStoreMockThatThrowsException(ex);
            var repositories = new Dictionary<Repositories, IWeatherRepositoryStore>()
                {{Repositories.Accuweather, repositoryStoreMock.Object}};
            var service = new WeatherService(null, repositories, _mapper);

            return service;
        }
    }
}
