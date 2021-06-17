using System;
using Xunit;
using System.Threading.Tasks;
using Moq;
using WeatherStation.App.Utilities;

namespace WeatherStation.App.Tests.UtilitiesTests
{
    public class ExceptionHandlingServiceTests
    {
        [Fact]
        public async Task HandleException_ValidCall_LogsMessage()
        {
            var mocks = CreateMocks();
            var exception = new Exception("Message");
            var handler = new ExceptionHandlingService(mocks.alertService.Object, mocks.logger.Object);

            await handler.HandleException(exception);

            mocks.logger.Verify(x => x.Log(It.IsAny<string>()), Times.Once);
        }

        private (Mock<IErrorAlertService> alertService,Mock<ILogger> logger) CreateMocks()
        {
            var alertServiceMock = new Mock<IErrorAlertService>();
            var loggerMock = new Mock<ILogger>();
            return (alertServiceMock, loggerMock);
        }

        [Fact]
        public async Task HandleException_ValidCall_DisplaysExceptionMessage()
        {
            var mocks = CreateMocks();
            var exception = new Exception("Message");
            var handler = new ExceptionHandlingService(mocks.alertService.Object, mocks.logger.Object);

            await handler.HandleException(exception);

            mocks.alertService.Verify(x => x.DisplayExceptionMessage(exception), Times.Once);
        }

        [Fact]
        public async Task HandleException_ExceptionIsNull_LogsMessage()
        {
            var mocks = CreateMocks();
            var handler = new ExceptionHandlingService(mocks.alertService.Object, mocks.logger.Object);

            await handler.HandleException(null);

            mocks.logger.Verify(x => x.Log("Unknown Error \n"), Times.Once);
        }

        [Fact]
        public async Task HandleException_ExceptionIsNull_DisplaysAlert()
        {
            var mocks = CreateMocks();
            var handler = new ExceptionHandlingService(mocks.alertService.Object, mocks.logger.Object);

            await handler.HandleException(null);

            mocks.alertService.Verify(x => x.DisplayAlert("Unknown Error", "Unknown Error"), Times.Once);
        }
    }
}