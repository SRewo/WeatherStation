using Xunit;
using System.Threading.Tasks;
using RestSharp;
using WeatherStation.Library.Repositories;
using Moq;
using System.Threading;

namespace WeatherStation.Library.Tests
{
    public class PositionstackGeocodingRepositoryTests
    {
        [Fact]
        public async Task GetLocationCoordinates_ProperExecution_ReturnsValidLatitude()
        {
            var client = CreateRestClientMock();
            var repository = new PositionstackGeocodingRepository(client.Object, "");

            var result = await repository.GetLocationCoordinates("Pszczyna");

            Assert.Equal(42.32, result.Latitude);
        }

        private Mock<IRestClient> CreateRestClientMock()
        {
            var client = new Mock<IRestClient>();
            var responseMock = CreateResponseMock();
            client.Setup(x => x.ExecuteAsync(It.IsAny<IRestRequest>(), CancellationToken.None)).ReturnsAsync(responseMock.Object);
            return client;
        }
                
        private Mock<IRestResponse> CreateResponseMock()
        {
            var response = new Mock<IRestResponse>();
            response.Setup(x => x.IsSuccessful).Returns(true);
            response.Setup(x => x.Content).Returns(CreateJsonResponse());
            return response; 
        }

        private string CreateJsonResponse()
        {
            return "{data:[{latitude:42.32, longitude:33.22}]}";
        }

        [Fact]
        public async Task GetLocationCoordinates_ProperExecution_ReturnsValidLongitude()
        {
            var client = CreateRestClientMock();
            var repository = new PositionstackGeocodingRepository(client.Object, "");

            var result = await repository.GetLocationCoordinates("Pszczyna");

            Assert.Equal(33.22, result.Longitude);
        }

        [Fact]
        public async Task GetLocationCoordinates_LocationDataStringIsEmpty_ThrowsException()
        {
            var client = CreateRestClientMock();
            var repository = new PositionstackGeocodingRepository(client.Object, "");

            await Assert.ThrowsAnyAsync<NoLocationDataException>(async () => { await repository.GetLocationCoordinates(""); });
        }
    }
}
