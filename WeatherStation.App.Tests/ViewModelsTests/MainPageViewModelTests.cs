using Grpc.Core;
using Moq;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using WeatherStation.App.Utilities;
using WeatherStation.App.ViewModels;
using WeatherStation.Library.Interfaces;
using Xamarin.Essentials.Interfaces;
using Xunit;
using static WeatherStation.App.Weather;

namespace WeatherStation.App.Tests.ViewModelsTests
{
    public class MainPageViewModelTests
    {
        [Fact]
        public async Task PerformRequiredTasks_WeatherDataIsNull_GetsCurrentWeatherDataFromRepository()
        {
            var clientMock = CreateClientMock();
            var parameters = new NavigationParameters { { "repository", Repositories.Accuweather } };
            var model = CreateViewModel(clientMock);
            model.WeatherData = null;

            await model.PerformRequiredTasks(parameters);

            clientMock.Verify(x => x.GetCurrentWeatherAsync(It.IsAny<WeatherRequest>(), null, null, default), Times.Once);
        }

        private Mock<WeatherClient> CreateClientMock()
        {
            var clientMock = new Mock<WeatherClient>();
            SetupRepositoryInfo(clientMock);
            SetupCurrentWeatherReply(clientMock);
            SetupDailyForecastsReply(clientMock);
            SetupHourlyForecastsReply(clientMock);

            return clientMock;
        }

        private void SetupRepositoryInfo(Mock<WeatherClient> clientMock)
        {
            var infoReply = Task.FromResult(new InfoReply
            {
                ContainsDailyForecasts = true,
                ContainsHourlyForecasts = true
            });
            clientMock.Setup(
                x => x.GetRepositoryInfoAsync(It.IsAny<InfoRequest>(), null, null, default))
                .Returns(new AsyncUnaryCall<InfoReply>(infoReply, null, null, null, null));
        }

        private void SetupCurrentWeatherReply(Mock<WeatherClient> clientMock)
        {
            clientMock.Setup(
                x => x.GetCurrentWeatherAsync(It.IsAny<WeatherRequest>(), null, null, default))
                .Returns(new AsyncUnaryCall<CurrentWeatherReply>(ReplyFactory.CreateCurrentWeatherReply(), null, null, null, null));
        }

        private void SetupDailyForecastsReply(Mock<WeatherClient> clientMock)
        {
            clientMock.Setup(
                x => x.GetDailyForecastsAsync(It.IsAny<WeatherRequest>(), null, null, default))
                .Returns(new AsyncUnaryCall<ForecastsReply>(ReplyFactory.CreateForecastsReply(10), null, null, null, null));
        }

        private void SetupHourlyForecastsReply(Mock<WeatherClient> clientMock)
        {
            clientMock.Setup(
                x => x.GetHourlyForecastsAsync(It.IsAny<WeatherRequest>(), null, null, default))
                .Returns(new AsyncUnaryCall<ForecastsReply>(ReplyFactory.CreateForecastsReply(10), null, null, null, null));
        }

        private MainPageViewModel CreateViewModel(Mock<WeatherClient> clientMock)
        {
            var dateProvider = PrepareDateProvider();
            var preferences = new Mock<IPreferences>();
            var handler = new Mock<IExceptionHandlingService>();
            return new MainPageViewModel(dateProvider.Object, preferences.Object, handler.Object, clientMock.Object);
        }

        private MainPageViewModel CreateViewModel(Mock<WeatherClient> clientMock, Mock<IExceptionHandlingService> serviceMock)
        {
            var dateProvider = PrepareDateProvider();
            var preferences = new Mock<IPreferences>();
            return new MainPageViewModel(dateProvider.Object, preferences.Object, serviceMock.Object, clientMock.Object);
        }

        private Mock<IDateProvider> PrepareDateProvider()
        {
            var dateProviderMock = new Mock<IDateProvider>();
            var mockedCurrentDatetime = new DateTime(2020, 01, 01, 12, 00, 00);
            dateProviderMock.Setup(x => x.GetActualDateTime()).Returns(mockedCurrentDatetime);
            return dateProviderMock;
        }

        [Fact]
        public async Task PerformRequiredTasks_GetsDailyForecasts()
        {
            var mock = CreateClientMock();
            var parameters = new NavigationParameters() { { "repository", Repositories.Accuweather } };
            var model = CreateViewModel(mock);

            await model.PerformRequiredTasks(parameters);

            mock.Verify(x => x.GetDailyForecastsAsync(It.IsAny<WeatherRequest>(), null, null, default), Times.Once);
        }

        [Fact]
        public async Task PerformRequiredTasks_GetsHourlyForecasts()
        {
            var mock = CreateClientMock();
            var parameters = new NavigationParameters() { { "repository", Repositories.Accuweather } };
            var model = CreateViewModel(mock);

            await model.PerformRequiredTasks(parameters);

            mock.Verify(x => x.GetHourlyForecastsAsync(It.IsAny<WeatherRequest>(), null, null, default), Times.Once);
        }

        [Fact]
        public async Task PerformRequiredTasks_RepositoryContainsDailyForecasts_SetsPropertyToTrue()
        {
            var mock = CreateClientMock();
            var parameters = new NavigationParameters() { { "repository", Repositories.Accuweather } };
            var model = CreateViewModel(mock);

            await model.PerformRequiredTasks(parameters);

            Assert.True(model.ContainsDailyForecasts);
        }

        [Fact]
        public async Task PerformRequiredTasks_RepositoryContainsHourlyForecasts_SetsPropertyToTrue()
        {
            var mock = CreateClientMock();
            var parameters = new NavigationParameters() { { "repository", Repositories.Accuweather } };
            var model = CreateViewModel(mock);

            await model.PerformRequiredTasks(parameters);

            Assert.True(model.ContainsHourlyForecasts);
        }

        [Fact]
        public async Task PerformRequiredTasks_ThrowsException_ExceptionHandled()
        {
            var clientMock = CreateClientMock();
            clientMock.Setup(x => x.GetRepositoryInfoAsync(It.IsAny<InfoRequest>(), null, null, default)).Throws(new RpcException(Status.DefaultCancelled));
            var parameters = new NavigationParameters() { { "repository", Repositories.Accuweather } };
            var serviceMock = new Mock<IExceptionHandlingService>();
            var model = CreateViewModel(clientMock, serviceMock);

            await model.PerformRequiredTasks(parameters);

            serviceMock.Verify(x => x.HandleException(It.IsAny<RpcException>()));
        }

        [Fact]
        public async Task PerformRequiredTasks_CreatesChart()
        {
            var mock = CreateClientMock();
            var parameters = new NavigationParameters() { { "repository", Repositories.Accuweather } };
            var model = CreateViewModel(mock);

            await model.PerformRequiredTasks(parameters);

            Assert.NotNull(model.Chart);
        }

        [Fact]
        public async Task PerformRequiredTasks_SetsForecastsTitle()
        {
            var mock = CreateClientMock();
            var parameters = new NavigationParameters() { { "repository", Repositories.Accuweather } };
            var model = CreateViewModel(mock);

            await model.PerformRequiredTasks(parameters);

            Assert.NotEmpty(model.ForecastsTitle);
        }

        [Fact]
        public void RefreshDataCommand_GetsCurrentWeatherData()
        {
            var mock = CreateClientMock();
            var model = CreateViewModel(mock);

            model.RefreshDataCommand.Execute();

            mock.Verify(x => x.GetCurrentWeatherAsync(It.IsAny<WeatherRequest>(), null, null, default), Times.Once);
        }

        [Fact]
        public void RefreshDataCommand_SetsWeatherDataProperty()
        {
            var mock = CreateClientMock();
            var model = CreateViewModel(mock);
            model.WeatherData = null;

            model.RefreshDataCommand.Execute();

            Assert.NotNull(model.WeatherData);
        }

        [Fact]
        public void RefreshDataCommand_GetsDailyForecasts()
        {
            var mock = CreateClientMock();
            var model = CreateViewModel(mock);
            model.ContainsDailyForecasts = true;

            model.RefreshDataCommand.Execute();

            mock.Verify(x => x.GetDailyForecastsAsync(It.IsAny<WeatherRequest>(), null, null, default), Times.Once);
        }

        [Fact]
        public void RefreshDataCommand_SetsDailyForecastsList()
        {
            var mock = CreateClientMock();
            var model = CreateViewModel(mock);
            model.WeatherDailyData = null;
            model.ContainsDailyForecasts = true;

            model.RefreshDataCommand.Execute();

            Assert.NotNull(model.WeatherDailyData);
        }

        [Fact]
        public void RefreshDataCommand_GetsHourlyForecasts()
        {
            var mock = CreateClientMock();
            var model = CreateViewModel(mock);
            model.ContainsHourlyForecasts = true;

            model.RefreshDataCommand.Execute();

            mock.Verify(x => x.GetHourlyForecastsAsync(It.IsAny<WeatherRequest>(), null, null, default), Times.Once);
        }

        [Fact]
        public void RefreshDataCommand_SetsHourlyForecastsList()
        {
            var mock = CreateClientMock();
            var model = CreateViewModel(mock);
            model.WeatherHourlyData = null;
            model.ContainsHourlyForecasts = true;

            model.RefreshDataCommand.Execute();

            Assert.NotNull(model.WeatherHourlyData);
        }

        [Fact]
        public void RefreshDataCommand_SetsChart()
        {
            var mock = CreateClientMock();
            var model = CreateViewModel(mock);
            model.Chart = null;
            model.ContainsDailyForecasts = true;

            model.RefreshDataCommand.Execute();

            Assert.NotNull(model.Chart);
        }
    }
}