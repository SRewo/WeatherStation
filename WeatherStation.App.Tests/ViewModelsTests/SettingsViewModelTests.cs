using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Moq;
using WeatherStation.App.ViewModels;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories.AccuWeather;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xunit;

namespace WeatherStation.App.Tests.ViewModelsTests
{
    public class SettingsViewModelTests
    {
        [Fact]
        public async Task SaveSettings_ProperExecution_SetsCityNameInPreferences()
        {
            var mock = AutoMock.GetLoose();
            var model = mock.Create<SettingsViewModel>();
            model.CityName = "Warsaw";

            await model.SaveSettings();

            mock.Mock<IPreferences>().Verify(x => x.Set("CityName", "Warsaw"), Times.Once);
        }

        [Fact]
        public async Task SaveSettings_ProperExecution_SetsRepositoryCityIdInPreferences()
        {
            var mock = AutoMock.GetLoose();
            mock.Mock<IWeatherRepositoryStore>().Setup(x => x.CityId).Returns("14433");
            mock.Mock<IWeatherRepositoryStore>().Setup(x => x.RepositoryName).Returns("AccuWeather");
            var model = mock.Create<SettingsViewModel>();
            model.CityName = "Warsaw";

            await model.SaveSettings();

            mock.Mock<IPreferences>().Verify(x => x.Set("AccuWeatherCityId", "14433"));
        }

        [Fact]
        public async Task SaveSettings_ProperExecution_CallsChangeCityMethodInRepositoryStore()
        {
            var mock = AutoMock.GetLoose();
            var model = mock.Create<SettingsViewModel>();
            model.CityName = "Warsaw";

            await model.SaveSettings();

            mock.Mock<IWeatherRepositoryStore>().Verify(x => x.ChangeCity("Warsaw"), Times.Once);
        }

        [Fact]
        public async Task SaveSettings_ProperExecution_DisplaysSettingsSavedAlert()
        {
            var mock = AutoMock.GetLoose();
            var model = mock.Create<SettingsViewModel>();
            model.CityName = "Warsaw";

            await model.SaveSettings();

            mock.Mock<IAlertService>().Verify(x => x.DisplayAlert("Settings Saved",It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SaveSettings_ChangeCityThrowsInvalidCityNameException_DisplaysErrorAlert()
        {
            var mock = AutoMock.GetLoose();
            var exceptionMessage = "Invalid city name.";
            var exception = new InvalidCityNameException(exceptionMessage);
            mock.Mock<IWeatherRepositoryStore>().Setup(x => x.ChangeCity("Warsaw")).Throws(exception);
            var model = mock.Create<SettingsViewModel>();
            model.CityName = "Warsaw";

            await model.SaveSettings();

            mock.Mock<IAlertService>().Verify(x => x.DisplayAlert(It.IsAny<string>(), exceptionMessage), Times.Once);
        }

        [Fact]
        public async Task SaveSettings_ChangeCityThrowsMultipleCitiesResultException_DisplaysErrorAlert()
        {
            var mock = AutoMock.GetLoose();
            var exceptionMessage = "Please provide additional city information.";
            var exception = new MultipleCitiesResultException(exceptionMessage);
            mock.Mock<IWeatherRepositoryStore>().Setup(x => x.ChangeCity("Warsaw")).Throws(exception);
            var model = mock.Create<SettingsViewModel>();
            model.CityName = "Warsaw";

            await model.SaveSettings();

            mock.Mock<IAlertService>().Verify(x => x.DisplayAlert(It.IsAny<string>(), exceptionMessage), Times.Once);
        }

        [Fact]
        public async Task SaveSettings_CityNameDidntChange_DoNotChangesCitiesInRepositoryStores()
        {
            var mock = AutoMock.GetLoose();
            mock.Mock<IPreferences>().Setup(x => x.Get("CityName", "")).Returns("Warsaw");
            var model = mock.Create<SettingsViewModel>();
            model.CityName = "Warsaw";

            await model.SaveSettings();

            mock.Mock<IWeatherRepositoryStore>().Verify(x => x.ChangeCity(It.IsAny<string>()),Times.Never);
        }

        [Fact]
        public async Task GetLocation_ProperExecution_SetsFlagToTrue()
        {
            var mock = await CreateAutoMockWithGeolocation();
            var model = mock.Create<SettingsViewModel>();

            await model.GetLocation();

            Assert.True(model.AreCoordinatesUsed);
        }

        private Task<AutoMock> CreateAutoMockWithGeolocation()
        {
            var mock = AutoMock.GetLoose();
            var location = new Location();
            mock.Mock<IGeolocation>().Setup(x => x.GetLocationAsync(It.IsAny<GeolocationRequest>()))
                .ReturnsAsync(location);
            return Task.FromResult(mock);
        }

        [Fact]
        public async Task GetLocation_CityNameWasChangedAfterExecution_CoordinatesUsedFlagIsSetToFalse()
        {
            var mock = await CreateAutoMockWithGeolocation();
            var model = mock.Create<SettingsViewModel>();
            await model.GetLocation();

            model.CityName = "Warsaw";

            Assert.False(model.AreCoordinatesUsed);
        }

        [Fact]
        public async Task GetLocation_ProperExecution_ChangesCityName()
        {
            var mock = await CreateAutoMockWithGeolocation();
            var placemark = new Placemark {Locality = "Warsaw", CountryName = "Poland"};
            var placemarkList = new List<Placemark> {placemark};
            mock.Mock<IGeocoding>().Setup(x => x.GetPlacemarksAsync(It.IsAny<Location>())).ReturnsAsync(placemarkList);
            var model = mock.Create<SettingsViewModel>();

            await model.GetLocation();

            var expected = "Warsaw,Poland";
            var actual = model.CityName;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetLocation_GeolocationMethodReturnsNull_ShowsAlert()
        {
            var mock = AutoMock.GetLoose();
            var model = mock.Create<SettingsViewModel>();

            await model.GetLocation();

            mock.Mock<IAlertService>().Verify(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetLocation_GeolocationThrowsPermissionException_ShowsAlert()
        {
            var mock = AutoMock.GetLoose();
            var exception = new PermissionException("Message");
            mock.Mock<IGeolocation>().Setup(x => x.GetLocationAsync(It.IsAny<GeolocationRequest>()))
                .Throws(exception);
            var model = mock.Create<SettingsViewModel>();

            await model.GetLocation();

            mock.Mock<IAlertService>().Verify(x => x.DisplayAlert("Permissions problem", exception.Message), Times.Once);
        }

        [Fact]
        public async Task GetLocation_GeolocationThrowsException_ShowsAlert()
        {
            var mock = AutoMock.GetLoose();
            var exception = new Exception("message");
            mock.Mock<IGeolocation>().Setup(x => x.GetLocationAsync(It.IsAny<GeolocationRequest>()))
                .Throws(exception);
            var model = mock.Create<SettingsViewModel>();

            await model.GetLocation();

            mock.Mock<IAlertService>().Verify(x => x.DisplayAlert(It.IsAny<string>(), exception.Message), Times.Once);
        }
    }
}