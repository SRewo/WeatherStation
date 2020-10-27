using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using RestSharp;
using WeatherStation.Library.Repositories;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories
{
    public class RestRequestHandlerTests
    {
        [Fact]
        public async Task GetDataFromApi_WorksProperly()
        {
            var responseMock = CreateResponseMock();
            var clientMock = CreateRestClientMock(responseMock.Object);
            var handler = new RestRequestHandler(clientMock.Object, "");

            var result = await handler.GetDataFromApi(new List<Parameter>());

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
            var handler = new RestRequestHandler(clientMock.Object, "");
            var parameters = new List<Parameter>();

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => handler.GetDataFromApi(parameters));

        }

        [Fact]
        public async Task GetDataFromApi_RequestWasNotSuccessful_ThrowsHttpRequestException()
        {
            var responseMock = CreateResponseMock();
            responseMock.Setup(x => x.IsSuccessful).Returns(false);
            var clientMock = CreateRestClientMock(responseMock.Object);
            var handler = new RestRequestHandler(clientMock.Object, "");
            var parameters = new List<Parameter>();

            await Assert.ThrowsAnyAsync<HttpRequestException>(() => handler.GetDataFromApi(parameters));
        }
    }
}