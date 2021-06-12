using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Prism.Navigation;
using WeatherStation.App.ViewModels;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using WeatherStation.App.Utilities;
using Xamarin.Essentials.Interfaces;
using Xunit;

namespace WeatherStation.App.Tests.ViewModelsTests
{
    public class MainPageViewModelTests
    {

        [Fact]
        public async Task PerformRequiredTasks_WeatherDataIsNull_GetsWeatherDataFromRepository()
        {
            var storeMock = new Mock<IWeatherRepositoryStore>();
            PrepareCurrentWeatherRepository(storeMock);
            var parameters = new NavigationParameters {{"repositoryStore", storeMock.Object}};
            var model = CreateViewModel();
            model.WeatherData = null;

            await model.PerformRequiredTasks(parameters);

            storeMock.Verify(x => x.CurrentWeatherRepository.GetWeatherDataFromRepository(), Times.Once);
        }

        private void PrepareCurrentWeatherRepository(Mock<IWeatherRepositoryStore> storeMock)
        {

            var weatherData = new WeatherData();
            var weatherDataList = new List<WeatherData> {weatherData};
            storeMock.Setup(x => x.CurrentWeatherRepository.GetWeatherDataFromRepository()).ReturnsAsync(weatherDataList);
        }

        private MainPageViewModel CreateViewModel()
        {
            var dateProvider = PrepareDateProvider();
            var preferences = new Mock<IPreferences>();
            var handler = new Mock<IExceptionHandlingService>();
            return new MainPageViewModel(dateProvider.Object, preferences.Object, handler.Object);
        }
        private MainPageViewModel CreateViewModel(Mock<IExceptionHandlingService> serviceMock)
        {
            var dateProvider = PrepareDateProvider();
            var preferences = new Mock<IPreferences>();
            return new MainPageViewModel(dateProvider.Object, preferences.Object, serviceMock.Object);
        }

        [Fact]
        public async Task PerformRequiredTasks_WeatherDataDateDiffIsLargerThan1Hour_GetsWeatherDataFromRepository()
        {
            var storeMock = new Mock<IWeatherRepositoryStore>();
            PrepareCurrentWeatherRepository(storeMock);
            var parameters = new NavigationParameters() {{"repositoryStore", storeMock.Object} };
            var model = CreateViewModel();
            var weatherDate = new DateTime(2020, 01, 01, 10, 59, 00);
            model.WeatherData = new WeatherData {Date = weatherDate};

            await model.PerformRequiredTasks(parameters);

            storeMock.Verify(x => x.CurrentWeatherRepository.GetWeatherDataFromRepository(), Times.Once);
        }

        private Mock<IDateProvider> PrepareDateProvider()
        {
            var dateProviderMock = new Mock<IDateProvider>();
            var mockedCurrentDatetime = new DateTime(2020, 01, 01, 12, 00, 00);
            dateProviderMock.Setup(x => x.GetActualDateTime()).Returns(mockedCurrentDatetime);
            return dateProviderMock;
        }

        [Fact]
        public async Task PerformRequiredTasks_WeatherDataDateDiffIsSmallerThan1Hour_WeatherDataDoesNotChange()
        {
            var storeMock = new Mock<IWeatherRepositoryStore>();
            PrepareCurrentWeatherRepository(storeMock);
            var parameters = new NavigationParameters() {{"repositoryStore", storeMock.Object} };
            var model = CreateViewModel();
            var weatherDate = new DateTime(2020, 01, 01, 11, 59, 00);
            model.WeatherData = new WeatherData {Date = weatherDate};

            await model.PerformRequiredTasks(parameters);


            storeMock.Verify(x => x.CurrentWeatherRepository.GetWeatherDataFromRepository(), Times.Never);
        }

        [Fact]
        public async Task PerformRequiredTasks_RepositoryContainsDailyForecasts_SetsPropertyToTrue()
        {
            var repositoryMock = new Mock<IWeatherRepositoryStore>();
            var dailyRepository = new Mock<IWeatherRepository>();
            repositoryMock.Setup(x => x.DailyForecastsRepository).Returns(dailyRepository.Object);
            var parameters = new NavigationParameters() {{"repositoryStore", repositoryMock.Object} };
            var model = CreateViewModel();
            await model.GetVariablesFromParameters(parameters);

            await model.CheckIfRepositoryContainsDailyAndHourlyForecasts();

            Assert.True(model.ContainsDailyForecasts);
        }

        [Fact]
        public async Task PerformRequiredTasks_RepositoryContainsHourlyForecasts_SetsPropertyToTrue()
        {
            var repositoryMock = new Mock<IWeatherRepositoryStore>();
            var hourlyForecasts = new Mock<IWeatherRepository>();
            repositoryMock.Setup(x => x.HourlyForecastsRepository).Returns(hourlyForecasts.Object);
            var parameters = new NavigationParameters() {{"repositoryStore", repositoryMock.Object} };
            var model = CreateViewModel();
            await model.GetVariablesFromParameters(parameters);

            await model.CheckIfRepositoryContainsDailyAndHourlyForecasts();

            Assert.True(model.ContainsHourlyForecasts);

        }

        private static Mock<IWeatherRepositoryStore> PrepareRepositoryMockWithHourlyForecasts()
        {
            var repositoryMock = new Mock<IWeatherRepositoryStore>();
            var hourlyRepositoryMock = new Mock<IWeatherRepository>();
            repositoryMock.Setup(x => x.HourlyForecastsRepository).Returns(hourlyRepositoryMock.Object);
            return repositoryMock;
        }

        [Fact]
        public async Task PerformRequiredTasks_ThrowsException_ExceptionHandled()
        {
            var repositoryMock = new Mock<IWeatherRepositoryStore>();
            var hourlyForecasts = new Mock<IWeatherRepository>();
            hourlyForecasts.Setup(x => x.GetWeatherDataFromRepository()).Throws(new Exception());
            repositoryMock.Setup(x => x.HourlyForecastsRepository).Returns(hourlyForecasts.Object);
            var parameters = new NavigationParameters() {{"repositoryStore", repositoryMock.Object} };
            var serviceMock = new Mock<IExceptionHandlingService>();
            var model = CreateViewModel(serviceMock);

            await model.PerformRequiredTasks(parameters);

            serviceMock.Verify(x => x.HandleException(It.IsAny<Exception>()));
        }

    }
}