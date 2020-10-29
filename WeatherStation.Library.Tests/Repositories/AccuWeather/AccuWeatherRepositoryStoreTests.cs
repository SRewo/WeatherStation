using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using WeatherStation.Library.Repositories.AccuWeather;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories
{
    public class AccuWeatherRepositoryTests
    {
        private const string WarsawCityCode = "274663";
        [Fact]
        public async Task ChangeCity_WithCityCoordinates_SetsCityCode()
        {
            var store = await CreateRepositoryStore();

            await store.ChangeCity(52.13f, 21f);

            var expected = WarsawCityCode;
            var actual = store.CityCode;
            Assert.Equal(expected, actual);
        }

        private static async Task<AccuWeatherRepositoryStore> CreateRepositoryStore()
        {
            var clientMock = await CreateRestClientMock();
            var dateProviderMock = new Mock<IDateProvider>();
            return AccuWeatherRepositoryStore.FromCityCode("", "",dateProviderMock.Object,clientMock.Object);
        }

        private static async Task< Mock<IRestClient>> CreateRestClientMock()
        {

            var clientMock = new Mock<IRestClient>();
            var responseMock = await CreateCitySearchResponseMock();
            clientMock.Setup(x => x.ExecuteAsync(It.IsAny<IRestRequest>(), CancellationToken.None)).ReturnsAsync(responseMock.Object);
            return clientMock;
        }

        private static async Task<Mock<IRestResponse>> CreateCitySearchResponseMock()
        {

            var responseMock = new Mock<IRestResponse>();
            var result = await LoadJsonResponse("Repositories/TestResponses/AccuWeatherCitySearchWithCoordinates.json");
            responseMock.Setup(x => x.Content).Returns(result);
            responseMock.Setup(x => x.IsSuccessful).Returns(true);
            return responseMock;
        }

        private static async Task<string> LoadJsonResponse(string path)
        {
            using var streamReader = new StreamReader(path);
            var result = await streamReader.ReadToEndAsync();
            return result;
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_SetsCityName()
        {
            var store = await CreateRepositoryStore();

            await store.ChangeCity(52.13f, 21);

            var expected = "Warsaw";
            var actual = store.CityName;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesCurrentWeatherRepository()
        {
            var store = await CreateRepositoryStore();
            var currentRepository = store.CurrentWeatherRepository;

            await store.ChangeCity(52.13f, 21);

            Assert.NotEqual(currentRepository, store.CurrentWeatherRepository);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesHourlyForecastsRepository()
        {
            var store = await CreateRepositoryStore();
            var forecastsRepository = store.HourlyForecastsRepository;

            await store.ChangeCity(52.13f, 21);

            Assert.NotEqual(forecastsRepository, store.HourlyForecastsRepository);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesDailyForecastRepository()
        {
            var store = await CreateRepositoryStore();
            var repositoryBeforeExecution = store.DailyForecastsRepository;

            await store.ChangeCity(52.13f, 21);

            var repositoryAfterExecution = store.DailyForecastsRepository;
            Assert.NotEqual(repositoryBeforeExecution, repositoryAfterExecution);
        }

        [Fact]
        public async Task ChangeCity_LatitudeTooSmall_ThrowsException()
        {
            var store = await CreateRepositoryStore();

            await Assert.ThrowsAnyAsync<LatitudeOutOfRangeException>(() => store.ChangeCity(-91f, 21));
        }

        [Fact]
        public async Task ChangeCity_LatitudeTooBig_ThrowsException()
        {
            var store = await CreateRepositoryStore();

            await Assert.ThrowsAnyAsync<LatitudeOutOfRangeException>(() => store.ChangeCity(91f, 21));
        }

        [Fact]
        public async Task ChangeCity_LongitudeTooSmall_ThrowsException()
        {
            var store = await CreateRepositoryStore();

            await Assert.ThrowsAnyAsync<LongitudeOutOfRangeException>(() => store.ChangeCity(52.13f, -181));
        }

        [Fact]
        public async Task ChangeCity_LongitudeTooBig_ThrowsException()
        {
            var store = await CreateRepositoryStore();

            await Assert.ThrowsAnyAsync<LongitudeOutOfRangeException>(() => store.ChangeCity(52.13f, 181));
        }
    }
}