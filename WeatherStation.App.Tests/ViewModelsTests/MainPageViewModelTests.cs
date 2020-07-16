using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Moq;
using RestSharp;
using WeatherStation.App.ViewModels;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using Xunit;

namespace WeatherStation.App.Tests.ViewModelsTests
{
    public class MainPageViewModelTests
    {
        [Fact]
        public void Constructor_WorksProperly()
        {
            var mock = AutoMock.GetLoose();

            var model = mock.Create<MainPageViewModel>();

            Assert.True(model.ContainsHourlyForecasts);
            Assert.True(model.ContainsDailyForecasts);
            Assert.NotNull(model.GetDataCommand);
        }

        [Fact]
        public void Constructor_BasicRepositoryIsNull_ThrowsNullReferenceException()
        {
            Assert.ThrowsAny<NullReferenceException>(() =>
            {
                var model = new MainPageViewModel(null, null, null, null);
            });

        }

        [Fact]
        public void Constructor_MissingHourlyForecastRepo_ContainsHourlyForecastsIsFalse()
        {
            var mock = AutoMock.GetLoose();

            var model = mock.Create<MainPageViewModel>(new NamedParameter("hourlyForecast", null));

            Assert.False(model.ContainsHourlyForecasts);
        }

        [Fact]
        public void Constructor_MissingDailyForecastRepo_ContainsDailyForecastsIsFalse()
        {
            var mock = AutoMock.GetLoose();

            var model = mock.Create<MainPageViewModel>(new NamedParameter("dailyForecast", null));

            Assert.False(model.ContainsDailyForecasts);
        }

        [Fact]
        public async Task GetData_WeatherDataIsNull_GetsWeatherDataFromRepository()
        {
            var mock = AutoMock.GetLoose();
            var builder = new WeatherDataBuilder();
            builder.SetChanceOfRain(40).SetPressure(999).SetTemperature(20.2f, TemperatureUnit.Celcius);
            mock.Mock<IBasicWeatherRepository>().Setup(x => x.GetCurrentWeather()).ReturnsAsync(builder.Build);
            var model = mock.Create<MainPageViewModel>();

            await model.GetData();

            mock.Mock<IBasicWeatherRepository>().Verify(x => x.GetCurrentWeather(), Times.Once);
            Assert.NotNull(model.WeatherData);
            Assert.Equal(40, model.WeatherData.ChanceOfRain);
            Assert.Equal(999, model.WeatherData.Pressure);
            Assert.Equal(20.2f, model.WeatherData.Temperature);
        }

        [Fact]
        public async Task GetData_WeatherDataDateDiffIsLargerThan30Min_GetsWeatherDataFromRepository()
        {
            var mock = AutoMock.GetLoose();
            var builder1 = new WeatherDataBuilder().SetDate(new DateTime(2020,01,01,12,00,00))
                .SetTemperature(20.4f, TemperatureUnit.Celcius);
            var builder2 = new WeatherDataBuilder().SetDate(new DateTime(2020,01,01,12,31,0))
                .SetTemperature(22.6f, TemperatureUnit.Celcius);
            mock.Mock<IBasicWeatherRepository>().Setup(x => x.GetCurrentWeather()).ReturnsAsync(builder2.Build);
            mock.Mock<IDateProvider>().Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01, 12, 31, 0));
            var model = mock.Create<MainPageViewModel>();
            model.WeatherData = builder1.Build();

            await model.GetData();

            mock.Mock<IBasicWeatherRepository>().Verify(x => x.GetCurrentWeather(), Times.Once);
            Assert.Equal(22.6f,model.WeatherData.Temperature);
        }

        [Fact]
        public async Task GetData_WeatherDataDateDiffIsSmallerThan30Min_WeatherDataDoesNotChange()
        {
            var mock = AutoMock.GetLoose();
            var builder1 = new WeatherDataBuilder().SetDate(new DateTime(2020,01,01,12,00,00))
                .SetTemperature(20.4f, TemperatureUnit.Celcius);
            var builder2 = new WeatherDataBuilder().SetDate(new DateTime(2020,01,01,12,31,0))
                .SetTemperature(22.6f, TemperatureUnit.Celcius);
            mock.Mock<IBasicWeatherRepository>().Setup(x => x.GetCurrentWeather()).ReturnsAsync(builder2.Build);
            mock.Mock<IDateProvider>().Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01, 12, 20, 0));
            var model = mock.Create<MainPageViewModel>();
            model.WeatherData = builder1.Build();

            await model.GetData();

            mock.Mock<IBasicWeatherRepository>().Verify(x => x.GetCurrentWeather(), Times.Never);
            Assert.Equal(20.4f,model.WeatherData.Temperature);
        }

        [Fact]
        public async Task GetData_ModelDoesContainDailyForecastRepo_CallsProperMethod()
        {
            var mock = AutoMock.GetLoose();
            var list = new List<WeatherData>{new WeatherData(), new WeatherData()};
            mock.Mock<IContainsDailyForecast>().Setup(x => x.GetDailyForecast()).ReturnsAsync(list);
            var model = mock.Create<MainPageViewModel>();

            await model.GetData();

            mock.Mock<IContainsDailyForecast>().Verify(x => x.GetDailyForecast(), Times.Once);
            Assert.NotEmpty(model.WeatherDailyData);
            Assert.Equal(2, model.WeatherDailyData.Count());
        }
      
        [Fact]
        public async Task GetData_ModelDoesNotContainDailyForecastRepo_SetsEmptyList()
        {
            var mock = AutoMock.GetLoose();
            var model = mock.Create<MainPageViewModel>(new NamedParameter("dailyForecast", null));

            await model.GetData();

            mock.Mock<IContainsDailyForecast>().Verify(x => x.GetDailyForecast(), Times.Never);
            Assert.Empty(model.WeatherDailyData);
        }
        
        [Fact]
        public async Task GetData_ModelDoesContainHourlyForecastRepo_CallsProperMethod()
        {
            var mock = AutoMock.GetLoose();
            var list = new List<WeatherData>{new WeatherData(), new WeatherData()};
            mock.Mock<IContainsHourlyForecast>().Setup(x => x.GetHourlyForecast()).ReturnsAsync(list);
            var model = mock.Create<MainPageViewModel>();

            await model.GetData();

            mock.Mock<IContainsHourlyForecast>().Verify(x => x.GetHourlyForecast(), Times.Once);
            Assert.NotEmpty(model.WeatherHourlyData);
            Assert.Equal(2, model.WeatherHourlyData.Count());
        }
      
        [Fact]
        public async Task GetData_ModelDoesNotContainHourlyForecastRepo_SetsEmptyList()
        {
            var mock = AutoMock.GetLoose();
            var model = mock.Create<MainPageViewModel>(new NamedParameter("hourlyForecast", null));

            await model.GetData();

            mock.Mock<IContainsHourlyForecast>().Verify(x => x.GetHourlyForecast(), Times.Never);
            Assert.Empty(model.WeatherHourlyData);
        }

        [Fact]
        public void OnNavigatedTo_GetsData()
        {
            var mock = AutoMock.GetLoose();
            var data = new WeatherDataBuilder().SetPressure(999).SetHumidity(50);
            var list1 = new List<WeatherData>{new WeatherData(), new WeatherData()};
            var list2 = new List<WeatherData>{new WeatherData(), new WeatherData(), new WeatherData()};
            mock.Mock<IContainsHourlyForecast>().Setup(x => x.GetHourlyForecast()).ReturnsAsync(list1);
            mock.Mock<IContainsDailyForecast>().Setup(x => x.GetDailyForecast()).ReturnsAsync(list2);
            mock.Mock<IBasicWeatherRepository>().Setup(x => x.GetCurrentWeather()).ReturnsAsync(data.Build);
            var model = mock.Create<MainPageViewModel>();

            model.OnNavigatedTo(null);

            Assert.NotNull(model.WeatherData);
            Assert.Equal(999, model.WeatherData.Pressure);
            Assert.Equal(50, model.WeatherData.Humidity);
            Assert.NotEmpty(model.WeatherDailyData);
            Assert.NotEmpty(model.WeatherHourlyData);

        }

    }
}