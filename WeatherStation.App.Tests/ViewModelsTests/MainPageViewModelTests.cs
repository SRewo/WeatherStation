using System;
using System.Threading.Tasks;
using Moq;
using Prism.Navigation;
using WeatherStation.App.ViewModels;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using Xunit;

namespace WeatherStation.App.Tests.ViewModelsTests
{
    public class MainPageViewModelTests : MainPageViewModel
        {

        [Fact]
        public async Task PerformRequiredTasks_WeatherDataIsNull_GetsWeatherDataFromRepository()
        {
            var mock = new Mock<IWeatherRepository>();
            var builder = new WeatherDataBuilder();
            mock.Setup(x => x.GetCurrentWeather()).ReturnsAsync(builder.Build);
            var parameters = new NavigationParameters {{"repository", mock.Object}};

            await PerformRequiredTasks(parameters);

            mock.Verify(x => x.GetCurrentWeather(), Times.Once);
        }


        [Fact]
        public async Task _WeatherDataDateDiffIsLargerThan1Hour_GetsWeatherDataFromRepository()
        {
            var repositoryMock = new Mock<IWeatherRepository>();
            var weatherDate = new DateTime(2020, 01, 01, 10, 59, 00);
            WeatherData = new WeatherDataBuilder().SetDate(weatherDate).Build();
            var parameters = new NavigationParameters() {{"repository", repositoryMock.Object} };
            PrepareDateProvider();

            await PerformRequiredTasks(parameters);

            repositoryMock.Verify(x => x.GetCurrentWeather(), Times.Once);
        }

        private void PrepareDateProvider()
        {
            var dateProviderMock = new Mock<IDateProvider>();
            var mockedCurrentDatetime = new DateTime(2020, 01, 01, 12, 00, 00);
            dateProviderMock.Setup(x => x.GetActualDateTime()).Returns(mockedCurrentDatetime);
            this.DateProvider = dateProviderMock.Object;
        }

        [Fact]
        public async Task PerformRequiredTasks_WeatherDataDateDiffIsSmallerThan1Hour_WeatherDataDoesNotChange()
        {
            var repositoryMock = new Mock<IWeatherRepository>();
            var weatherDate = new DateTime(2020, 01, 01, 11, 59, 00);
            WeatherData = new WeatherDataBuilder().SetDate(weatherDate).Build();
            var parameters = new NavigationParameters() {{"repository", repositoryMock.Object} };
            PrepareDateProvider();

            await PerformRequiredTasks(parameters);

            repositoryMock.Verify(x => x.GetCurrentWeather(), Times.Never);
        }

        [Fact]
        public async Task PerformRequiredTasks_RepositoryContainsDailyForecasts_SetsPropertyToTrue()
        {
            var repositoryMock = PrepareRepositoryMockWithDailyForecasts();
            var parameters = new NavigationParameters() {{"repository", repositoryMock.Object} };
            ContainsDailyForecasts = false;

            await PerformRequiredTasks(parameters);

            Assert.True(ContainsDailyForecasts);
        }

        private static Mock<IWeatherRepository> PrepareRepositoryMockWithDailyForecasts()
        {
            var repositoryMock = new Mock<IWeatherRepository>();
            var dailyRepositoryMock = new Mock<IContainsDailyForecast>();
            repositoryMock.Setup(x => x.DailyRepository).Returns(dailyRepositoryMock.Object);
            return repositoryMock;
        }


        [Fact]
        public async Task PerformRequiredTasks_RepositoryContainsHourlyForecasts_SetsPropertyToTrue()
        {
            var repositoryMock = PrepareRepositoryMockWithHourlyForecasts();
            var parameters = new NavigationParameters() {{"repository", repositoryMock.Object} };
            ContainsHourlyForecasts = false;

            await PerformRequiredTasks(parameters);

            Assert.True(ContainsHourlyForecasts);
        }

        private static Mock<IWeatherRepository> PrepareRepositoryMockWithHourlyForecasts()
        {
            var repositoryMock = new Mock<IWeatherRepository>();
            var hourlyRepositoryMock = new Mock<IContainsHourlyForecast>();
            repositoryMock.Setup(x => x.HourlyRepository).Returns(hourlyRepositoryMock.Object);
            return repositoryMock;
        }

    }
}