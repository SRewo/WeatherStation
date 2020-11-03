using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
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

        private const string CoordinatesSearchJsonFilePath =
            "Repositories/TestResponses/AccuWeatherCitySearchWithCoordinates.json";

        private const string SingleCityResultJsonFilePath =
            "Repositories/TestResponses/AccuWeatherCitySearchSingleCityResult.json";

        private const string MultipleCitiesResultJsonFilePath =
            "Repositories/TestResponses/AccuWeatherCitySearchMultipleCitiesResult.json";
        [Fact]
        public async Task ChangeCity_WithCityCoordinates_SetsCityCode()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await store.ChangeCity(52.13f, 21f);

            var expected = WarsawCityCode;
            var actual = store.CityId;
            Assert.Equal(expected, actual);
        }

        private static async Task<AccuWeatherRepositoryStore> CreateRepositoryStore(string jsonResponseFilePath)
        {
            var clientMock = await CreateRestClientMock(jsonResponseFilePath);
            var dateProviderMock = new Mock<IDateProvider>();
            return await AccuWeatherRepositoryStore.FromCityCode("", "",dateProviderMock.Object,clientMock.Object, Language.English);
        }

        private static async Task< Mock<IRestClient>> CreateRestClientMock(string jsonResponseFilePath)
        {

            var clientMock = new Mock<IRestClient>();
            var responseMock = await CreateCitySearchResponseMock(jsonResponseFilePath);
            clientMock.Setup(x => x.ExecuteAsync(It.IsAny<IRestRequest>(), CancellationToken.None)).ReturnsAsync(responseMock.Object);
            return clientMock;
        }

        private static async Task<Mock<IRestResponse>> CreateCitySearchResponseMock(string filePath)
        {

            var responseMock = new Mock<IRestResponse>();
            var result = await LoadJsonResponse(filePath);
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
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await store.ChangeCity(52.13f, 21);

            var expected = "Warsaw";
            var actual = store.CityName;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesCurrentWeatherRepository()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);
            var currentRepository = store.CurrentWeatherRepository;

            await store.ChangeCity(52.13f, 21);

            Assert.NotEqual(currentRepository, store.CurrentWeatherRepository);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesHourlyForecastsRepository()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);
            var forecastsRepository = store.HourlyForecastsRepository;

            await store.ChangeCity(52.13f, 21);

            Assert.NotEqual(forecastsRepository, store.HourlyForecastsRepository);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesDailyForecastRepository()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);
            var repositoryBeforeExecution = store.DailyForecastsRepository;

            await store.ChangeCity(52.13f, 21);

            var repositoryAfterExecution = store.DailyForecastsRepository;
            Assert.NotEqual(repositoryBeforeExecution, repositoryAfterExecution);
        }

        [Fact]
        public async Task ChangeCity_LatitudeTooSmall_ThrowsException()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await Assert.ThrowsAnyAsync<LatitudeOutOfRangeException>(() => store.ChangeCity(-91f, 21));
        }

        [Fact]
        public async Task ChangeCity_LatitudeTooBig_ThrowsException()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await Assert.ThrowsAnyAsync<LatitudeOutOfRangeException>(() => store.ChangeCity(91f, 21));
        }

        [Fact]
        public async Task ChangeCity_LongitudeTooSmall_ThrowsException()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await Assert.ThrowsAnyAsync<LongitudeOutOfRangeException>(() => store.ChangeCity(52.13f, -181));
        }

        [Fact]
        public async Task ChangeCity_LongitudeTooBig_ThrowsException()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await Assert.ThrowsAnyAsync<LongitudeOutOfRangeException>(() => store.ChangeCity(52.13f, 181));
        }

        [Fact]
        public async Task ChangeCityWithCityName_CityNameIsEmpty_ThrowsException()
        {
            var store = await CreateRepositoryStore(SingleCityResultJsonFilePath);

            await Assert.ThrowsAnyAsync<Exception>(() => store.ChangeCity(""));
        }

        [Fact]
        public async Task ChangeCityWithCityName_SearchReturnsMultipleCities_ThrowsException()
        {
            var store = await CreateRepositoryStore(MultipleCitiesResultJsonFilePath);

            await Assert.ThrowsAnyAsync<Exception>(() => store.ChangeCity("Warsaw"));
        }

        [Fact]
        public async Task ChangeCityWithCityName_RequestReturnsOnlyOneCity_ChangesCityCode()
        {
            var store = await CreateRepositoryStore(SingleCityResultJsonFilePath);

            await store.ChangeCity("Warsaw, Poland");

            var expected = WarsawCityCode;
            var actual = store.CityId;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeCityWithCityName_RequestReturnsOnlyOneCity_ChangesCityName()
        {
            var store = await CreateRepositoryStore(SingleCityResultJsonFilePath);

            await store.ChangeCity("Warsaw,Poland");

            var expected = "Warsaw";
            var actual = store.CityName;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeCityWithCityName_RequestReturnsOnlyOneCity_ChangesCurrentWeatherRepository()
        {
            var store = await CreateRepositoryStore(SingleCityResultJsonFilePath);
            var currentRepository = store.CurrentWeatherRepository;

            await store.ChangeCity("Warsaw,Poland");

            Assert.NotEqual(currentRepository, store.CurrentWeatherRepository);
        }

        [Fact]
        public async Task ChangeCityWithCityName_RequestReturnsOnlyOneCity_ChangesHourlyForecastsRepository()
        {
            var store = await CreateRepositoryStore(SingleCityResultJsonFilePath);
            var forecastsRepository = store.HourlyForecastsRepository;

            await store.ChangeCity("Warsaw,Poland");

            Assert.NotEqual(forecastsRepository, store.HourlyForecastsRepository);
        }

        [Fact]
        public async Task ChangeCityWithCityName_RequestReturnsOnlyOneCity_ChangesDailyForecastRepository()
        {
            var store = await CreateRepositoryStore(SingleCityResultJsonFilePath);
            var repositoryBeforeExecution = store.DailyForecastsRepository;

            await store.ChangeCity("Warsaw,Poland");

            var repositoryAfterExecution = store.DailyForecastsRepository;
            Assert.NotEqual(repositoryBeforeExecution, repositoryAfterExecution);
        }

        [Theory]
        [InlineData("W@rsaw")]
        [InlineData("Warsaw!")]
        [InlineData("___Warsaw")]
        [InlineData("#Warsaw")]
        public async Task ChangeCityWithCityName_CityNameContainsSpecialCharacters_ThrowsException(string cityName)
        {
            var store = await CreateRepositoryStore(SingleCityResultJsonFilePath);

            await Assert.ThrowsAnyAsync<Exception>(() => store.ChangeCity(cityName));
        }

        [Fact]
        public async Task ChangeLanguage_ValidExecution_ChangesCurrentWeatherRepositoryLanguageProperty()
        {
            var store = await AccuWeatherRepositoryStore.FromCityCode("", "", null, null, Language.Polish);

            await store.ChangeLanguage(Language.English);

            var expected = "en-EN";
            var actual = store.CurrentWeatherRepository.Language;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeLanguage_ValidExecution_ChangesHourlyForecastsRepositoryLanguageProperty()
        {
            var store = await AccuWeatherRepositoryStore.FromCityCode("", "", null, null, Language.Polish);

            await store.ChangeLanguage(Language.English);

            var expected = "en-EN";
            var actual = store.HourlyForecastsRepository.Language;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeLanguage_ValidExecution_ChangesDailyForecastsRepositoryLanguageProperty()
        {
            var store = await AccuWeatherRepositoryStore.FromCityCode("", "", null, null, Language.Polish);

            await store.ChangeLanguage(Language.English);

            var expected = "en-EN";
            var actual = store.DailyForecastsRepository.Language;
            Assert.Equal(expected, actual);
        }
    }
}