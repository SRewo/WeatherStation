using System.Threading.Tasks;
using Xunit;
using Moq;
using WeatherStation.App.Utilities;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.App.Tests.UtilitiesTests
{
    public class LoggerTests
    {
        [Fact]
        public async Task Log_ValidCall_ExecutesPropperMethod(){
            var mocks = CreateMocks();
            var logger = new Logger(mocks.writeService.Object, mocks.dateProvider.Object);

            await logger.Log("123");

            mocks.writeService.Verify(x => x.WriteFromString(It.IsAny<string>()), Times.Once);
        }

        private (Mock<IDateProvider> dateProvider, Mock<IWriteService> writeService) CreateMocks(){
            var dateMock = new Mock<IDateProvider>();
            dateMock.Setup(x => x.GetActualDateTime()).Returns(new System.DateTime(2021,01,01,1,1,1));
            var writeServiceMock = new Mock<IWriteService>();
            return (dateMock, writeServiceMock);
        }

        [Fact]
        public async Task Log_EmptyString_ExecutesPropperMethod()
        {
            var mocks = CreateMocks();
            var logger = new Logger(mocks.writeService.Object, mocks.dateProvider.Object);

            await logger.Log(string.Empty);

            mocks.writeService.Verify(x => x.WriteFromString(It.IsAny<string>()), Times.Once);
        }
    }
}