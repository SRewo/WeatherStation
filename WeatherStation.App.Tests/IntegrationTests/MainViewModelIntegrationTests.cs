using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WeatherStation.App.ViewModels;
using Xunit;

namespace WeatherStation.App.Tests.IntegrationTests
{
    public class MainViewModelIntegrationTests : IClassFixture<MainVMIntegrationTestFixture>
    {
        private MainVMIntegrationTestFixture _fixture;

        public MainViewModelIntegrationTests(MainVMIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PerformRequiredTasks_DownloadsCurrentWeatherData_ValidCall()
        {
            var model = _fixture.Container.Resolve<MainPageViewModel>();
            var parameters = new NavigationParameters { { "repository", Repositories.Accuweather } };

            await model.PerformRequiredTasks(parameters);

            Assert.NotNull(model.WeatherData);
        }

        [Fact]
        public async Task PerformRequiredTasks_DownloadsDailyForecasts_ValidCall()
        {
            var model = _fixture.Container.Resolve<MainPageViewModel>();
            var parameters = new NavigationParameters { { "repository", Repositories.Accuweather } };

            await model.PerformRequiredTasks(parameters);

            Assert.NotEmpty(model.WeatherDailyData);
        }

        [Fact]
        public async Task PerformRequiredTasks_DownloadsHourlyForecasts_ValidCall()
        {
            var model = _fixture.Container.Resolve<MainPageViewModel>();
            var parameters = new NavigationParameters { { "repository", Repositories.Accuweather } };

            await model.PerformRequiredTasks(parameters);

            Assert.NotEmpty(model.WeatherHourlyData);
        }
    }
}
