using Moq;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using WeatherStation.App.Utilities;
using WeatherStation.App.ViewModels;
using static WeatherStation.App.Weather;

namespace WeatherStation.App.Tests.ViewModelsTests
{
    public class MasterPageVMTestsFixture : IAsyncDisposable
    {
        internal Mock<INavigationService> navigationService;
        internal Mock<IExceptionHandlingService> exceptionHandlingService;

        public MasterPageVMTestsFixture()
        {
            navigationService = new Mock<INavigationService>(); 
            exceptionHandlingService = new Mock<IExceptionHandlingService>();
        }

        internal MasterPageViewModel CreateViewModel()
        {
            return new MasterPageViewModel(navigationService.Object, exceptionHandlingService.Object);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
