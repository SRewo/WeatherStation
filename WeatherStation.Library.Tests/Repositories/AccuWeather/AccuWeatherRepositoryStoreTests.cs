using System.Threading.Tasks;
using Moq;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories.AccuWeather;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories
{
    public class AccuWeatherRepositoryTests
    {
        private const string WarsawCityCode = "274663";

        private const string CoordinatesSearchJsonFilePath =
            "Repositories/AccuWeather/TestResponses/AccuWeatherCitySearchWithCoordinates.json";

        private Coordinates _coordinates = new Coordinates(10,21);

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_SetsCityCode()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await store.ChangeCity(_coordinates);

            var expected = WarsawCityCode;
            var actual = store.CityId;
            Assert.Equal(expected, actual);
        }

        private static async Task<AccuWeatherRepositoryStore> CreateRepositoryStore(string jsonResponseFilePath)
        {
            var clientMock = await CommonMethods.CreateRestClientMock(jsonResponseFilePath);
            var dateProviderMock = new Mock<IDateProvider>();
            return await AccuWeatherRepositoryStore.FromCityCode("", "",dateProviderMock.Object,clientMock.Object, "en-US");
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_SetsCityName()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);

            await store.ChangeCity(_coordinates);

            var expected = "Warsaw";
            var actual = store.CityName;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesCurrentWeatherRepository()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);
            var currentRepository = store.CurrentWeatherRepository;

            await store.ChangeCity(_coordinates);

            Assert.NotEqual(currentRepository, store.CurrentWeatherRepository);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesHourlyForecastsRepository()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);
            var forecastsRepository = store.HourlyForecastsRepository;

            await store.ChangeCity(_coordinates);

            Assert.NotEqual(forecastsRepository, store.HourlyForecastsRepository);
        }

        [Fact]
        public async Task ChangeCity_WithCityCoordinates_ChangesDailyForecastRepository()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);
            var repositoryBeforeExecution = store.DailyForecastsRepository;

            await store.ChangeCity(_coordinates);

            var repositoryAfterExecution = store.DailyForecastsRepository;
            Assert.NotEqual(repositoryBeforeExecution, repositoryAfterExecution);
        }

        [Fact]
        public async Task ChangeCity_CoordinatesAreNotValid_ThrowsException()
        {
            var store = await CreateRepositoryStore(CoordinatesSearchJsonFilePath);
            var mock = new Mock<Coordinates>(10,10);
            mock.Setup(x => x.IsValid()).Returns(false);

            await Assert.ThrowsAnyAsync<InvalidCoordinatesException>(() => store.ChangeCity(mock.Object));
        }

        [Fact]
        public async Task ChangeLanguage_ValidExecution_ChangesCurrentWeatherRepositoryLanguageProperty()
        {
            var store = await AccuWeatherRepositoryStore.FromCityCode("", "", null, null, "pl-PL");

            await store.ChangeLanguage("en-EN");

            var expected = "en-EN";
            var actual = store.CurrentWeatherRepository.Language;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeLanguage_ValidExecution_ChangesHourlyForecastsRepositoryLanguageProperty()
        {
            var store = await AccuWeatherRepositoryStore.FromCityCode("", "", null, null, "pl-PL");

            await store.ChangeLanguage("en-EN");

            var expected = "en-EN";
            var actual = store.HourlyForecastsRepository.Language;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ChangeLanguage_ValidExecution_ChangesDailyForecastsRepositoryLanguageProperty()
        {
            var store = await AccuWeatherRepositoryStore.FromCityCode("", "", null, null, "pl-PL");

            await store.ChangeLanguage("en-EN");

            var expected = "en-EN";
            var actual = store.DailyForecastsRepository.Language;
            Assert.Equal(expected, actual);
        }
    }
}