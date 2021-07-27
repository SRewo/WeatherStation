using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Tynamix.ObjectFiller;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Services.Tests
{
    public static class TestMockFactory
    {
        
        internal static async Task<Mock<IWeatherRepositoryStore>> CreateRepositoryStoreMock(bool containsDailyForecasts, bool containsHourlyForecasts)
        {
            var repository = new Mock<IWeatherRepositoryStore>();
            await PrepareRepositorySetups(repository, (containsDailyForecasts, containsHourlyForecasts));

            return repository;
        }

        private static async Task PrepareRepositorySetups(Mock<IWeatherRepositoryStore> repository, (bool dailyForecasts, bool HourlyForecasts) contains)
        {
            repository.Setup(x => x.RepositoryName).Returns("Testing");
            repository.Setup(x => x.ContainsHistoricalData).Returns(false);
            repository.Setup(x => x.ContainsDailyForecasts).Returns(contains.dailyForecasts);
            repository.Setup(x => x.ContainsHourlyForecasts).Returns(contains.HourlyForecasts);
            repository.Setup(x => x.CurrentWeatherRepository).Returns(await PrepareCurrentWeatherRepository());
            repository.Setup(x => x.DailyForecastsRepository)
                .Returns(await PrepareDailyForecastRepository(contains.dailyForecasts));
        }

        private static async Task<IWeatherRepository> PrepareCurrentWeatherRepository()
        {
            var repository = new Mock<IWeatherRepository>();
            var filler = await CreateWeatherDataFiller();
            repository.Setup(x => x.GetWeatherDataFromRepository()).ReturnsAsync(filler.Create(1));
            return repository.Object;
        }

        private static async Task<Filler<WeatherData>> CreateWeatherDataFiller()
        {
            var filler = new Filler<WeatherData>();
            var tempFiller = await CreateTemperatureFiller();
            filler.Setup()
                .OnProperty(x => x.ChanceOfRain).Use(Enumerable.Range(0, 100))
                .OnType<Temperature>().Use(tempFiller.Create)
                .OnProperty(x => x.Humidity).Use(Enumerable.Range(0,100))
                .OnProperty(x => x.Date).Use(new DateTimeRange(new DateTime(2021,01,01,01,01,01), new DateTime(2021,07,01,01,01,01)))
                .OnProperty(x => x.PrecipitationSummary).Use(new FloatRange(0,200))
                .OnProperty(x => x.Pressure).Use(Enumerable.Range(900,200))
                .OnProperty(x => x.WeatherDescription).Use(new MnemonicString(2,4,10));

            return filler;
        }

        private static Task<Filler<CelsiusTemperature>> CreateTemperatureFiller()
        {
            var filler = new Filler<CelsiusTemperature>();
            filler.Setup()
                .OnProperty(x => x.Value).Use(new FloatRange(-30, 40))
                .OnProperty(x => x.Scale).Use(Library.TemperatureScale.Celsius);

            return Task.FromResult(filler);
        }

        private static async Task<IWeatherRepository> PrepareDailyForecastRepository(bool isNotNull)
        {
            if (!isNotNull)
                return null;

            var repository = new Mock<IWeatherRepository>();
            var filler = await CreateWeatherDataFiller();
            repository.Setup(x => x.GetWeatherDataFromRepository()).ReturnsAsync(filler.Create(10));

            return repository.Object;

        }

        internal static async Task<Mock<IWeatherRepositoryStore>> CreateRepositoryStoreMockThatThrowsException(
            Exception ex)
        {
            var repositoryStoreMock = new Mock<IWeatherRepositoryStore>();
            await PrepareRepositoriesWithExceptionThrowing(repositoryStoreMock, ex);

            return repositoryStoreMock;
        }

        private static Task PrepareRepositoriesWithExceptionThrowing(Mock<IWeatherRepositoryStore> mock, Exception ex)
        {
            mock.Setup(x => x.CurrentWeatherRepository.GetWeatherDataFromRepository()).Throws(ex);
            mock.Setup(x => x.DailyForecastsRepository.GetWeatherDataFromRepository()).Throws(ex);
            mock.Setup(x => x.HourlyForecastsRepository.GetWeatherDataFromRepository()).Throws(ex);
            mock.Setup(x => x.ContainsDailyForecasts).Returns(true);
            mock.Setup(x => x.ContainsHourlyForecasts).Returns(true);
            
            return Task.CompletedTask;
        }
    }
}