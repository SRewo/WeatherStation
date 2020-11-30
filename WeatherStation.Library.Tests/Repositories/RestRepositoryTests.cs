using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using RestSharp;
using WeatherStation.Library.Repositories;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories
{
    public class RestRepositoryTests : RestRepository
    {
        [Fact]
        public async Task GetDataFromApi_WorksProperly()
        {
            var responseMock = CreateResponseMock();
            var clientMock = CreateRestClientMock(responseMock.Object);
            var handler = new RestRepositoryTests(clientMock.Object, "");

            var result = await handler.GetDataFromApi();

            Assert.NotNull(result);
        }

        private Mock<IRestResponse> CreateResponseMock()
        {
            var responseMock = new Mock<IRestResponse>();
            var responseBody = "[{Humidity:10}]";
            responseMock.Setup(x => x.Content).Returns(responseBody);
            responseMock.Setup(x => x.IsSuccessful).Returns(true);
            return responseMock;
        }

        private Mock<IRestClient> CreateRestClientMock(IRestResponse response)
        {
            var clientMock = new Mock<IRestClient>();
            clientMock.Setup(x => x.ExecuteAsync(It.IsAny<IRestRequest>(), CancellationToken.None))
                .ReturnsAsync(response);
            return clientMock;
        }

        [Fact]
        public async Task GetDataFromApi_ResponseContentIsEmpty_ThrowsNullReferenceException()
        {
            var responseMock = CreateResponseMock();
            responseMock.Setup(x => x.Content).Returns("");
            var clientMock = CreateRestClientMock(responseMock.Object);
            var handler = new RestRepositoryTests(clientMock.Object, "");

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => handler.GetDataFromApi());

        }

        [Fact]
        public async Task GetDataFromApi_RequestWasNotSuccessful_ThrowsHttpRequestException()
        {
            var responseMock = CreateResponseMock();
            responseMock.Setup(x => x.IsSuccessful).Returns(false);
            var clientMock = CreateRestClientMock(responseMock.Object);
            var handler = new RestRepositoryTests(clientMock.Object, "");

            await Assert.ThrowsAnyAsync<HttpRequestException>(() => handler.GetDataFromApi());
        }

        private RestRepositoryTests(IRestClient client, string resourcePath) : base(client, resourcePath)
        {

        }

        public RestRepositoryTests() : base(null, "")
        {

        }

        protected override Task AddParametersToRequest(IRestRequest request)
        {
            return Task.CompletedTask;
        }
    }
}