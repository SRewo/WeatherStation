using Moq;
using RestSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library.Tests.Repositories
{
    public static class CommonMethods
    {
        public static async Task<Mock<IRestClient>> CreateRestClientMock(string jsonFileLocation)
        {
            var clientMock = new Mock<IRestClient>();
            var responseMock = await CreateResponseMock(jsonFileLocation);
            clientMock.Setup(x => x.ExecuteAsync(It.IsAny<IRestRequest>(), CancellationToken.None)).ReturnsAsync(responseMock.Object);
            return clientMock;
        }

        public static Mock<IDateProvider> CreateDateProviderMock()
        {
            var dateProviderMock = new Mock<IDateProvider>();
            dateProviderMock.Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01));
            return dateProviderMock;
        }

        private static async Task<Mock<IRestResponse>> CreateResponseMock(string jsonFileLocation)
        {
            var mock = new Mock<IRestResponse>();
            var jsonResult = await LoadJsonResponse(jsonFileLocation);
            mock.Setup(x => x.Content).Returns(jsonResult);
            mock.Setup(x => x.IsSuccessful).Returns(true);
            return mock;
        }

        private static async Task<string> LoadJsonResponse(string path)
        {
            using var streamReader = new StreamReader(path);
            var result = await streamReader.ReadToEndAsync();
            return result;
        }

    }
}
