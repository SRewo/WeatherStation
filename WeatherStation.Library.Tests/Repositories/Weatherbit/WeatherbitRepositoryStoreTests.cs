using Moq;
using RestSharp;
using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories.Weatherbit;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories.Weatherbit
{
    public class WeatherbitRepositoryStoreTests
    {
        [Fact]
        public async Task ChangeCity_ValidCall_ChangesCurrentWeatherRepository()
        {
            var store = await CreateTestRepositoryStore(); 
            var currentRepository = store.CurrentWeatherRepository;

            await store.ChangeCity(new Coordinates(10,10));

            Assert.NotEqual(currentRepository, store.CurrentWeatherRepository);
        }

        private Task<WeatherbitRepositoryStore> CreateTestRepositoryStore()
        {
            var clientMock = new Mock<IRestClient>();
            var providerMock = new Mock<IDateProvider>();
            var store = new WeatherbitRepositoryStore(clientMock.Object,"", new Coordinates(1,2),providerMock.Object, "pl");
            return Task.FromResult(store);
        }

        [Fact]
        public async Task ChangeCity_ValidCall_ChangesDailyForecastsRepository()
        {
            var store = await CreateTestRepositoryStore();
            var currentRepository = store.DailyForecastsRepository;

            await store.ChangeCity(new Coordinates(10,10));

            Assert.NotEqual(currentRepository, store.DailyForecastsRepository);
        }

        [Fact]
        public async Task ChangeCity_ValidCall_ChangesHourlyForecastRepository()
        {
            var store = await CreateTestRepositoryStore();
            var currentRepository = store.HourlyForecastsRepository;

            await store.ChangeCity(new Coordinates(10,10));

            Assert.NotEqual(currentRepository, store.HourlyForecastsRepository);
        }

        [Fact]
        public async Task ChangeCity_InvaildCoordinates_ThrowsException()
        {
            var store = await CreateTestRepositoryStore();
            var coordinatesMock = new Mock<Coordinates>(10,10);
            coordinatesMock.Setup(x => x.IsValid()).Returns(false);

            await Assert.ThrowsAnyAsync<InvalidCoordinatesException>(() => store.ChangeCity(coordinatesMock.Object));
        }

        [Fact]
        public async Task ChangeLanguage_ValidCall_ChangesLanguageInCurrentWeatherRepository()
        {
            var store = await CreateTestRepositoryStore();

            await store.ChangeLanguage("en");

            Assert.Equal("en", store.CurrentWeatherRepository.Language);
        }

        [Fact]
        public async Task ChangeLanguage_ValidCall_ChangesLanguageInDailyForecastsRepository()
        {
            var store = await CreateTestRepositoryStore();

            await store.ChangeLanguage("en");

            Assert.Equal("en", store.DailyForecastsRepository.Language);
        }

        [Fact]
        public async Task ChangeLanguage_ValidCall_ChangesLanguageInHoulyForecastsRepository()
        {
            var store = await CreateTestRepositoryStore();

            await store.ChangeLanguage("en");

            Assert.Equal("en", store.HourlyForecastsRepository.Language);
        }
    }
}
