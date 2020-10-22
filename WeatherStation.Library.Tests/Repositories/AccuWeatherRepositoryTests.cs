using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using RestSharp;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories
{
    public class AccuWeatherRepositoryTests
    {
        private static string _cityCode = "1411530";

        [Fact]
        public async Task GetCurrentWeather_WorksProperly()
        {
            var clientMock = new Mock<IRestClient>();
            await SetupWeatherRestResponse(clientMock);
            var dateProviderMock = CreateDateProviderMock();
            var repository = AccuWeatherRepository.FromCityCode("", _cityCode, dateProviderMock.Object, clientMock.Object);

            var weather = await repository.GetCurrentWeather();

            Assert.NotNull(weather);
        }

        private static Mock<IDateProvider> CreateDateProviderMock()
        {
            var dateProviderMock = new Mock<IDateProvider>();
            dateProviderMock.Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01));
            return dateProviderMock;
        }

        private static async Task<Mock<IRestResponse>> SetupWeatherRestResponse(Mock<IRestClient> clientMock)
        {
            var response = await CreateCurrentWeatherRestResponseMock();
            AddCurrentWeatherResponseToRestClientMock(clientMock, response.Object);
            return response;
        }

        private static void AddCurrentWeatherResponseToRestClientMock(Mock<IRestClient> clientMock,IRestResponse response)
        {
           clientMock.Setup(x => x.ExecuteAsync(
                    It.Is<RestRequest>(z => z.Resource == $"currentconditions/v1/{_cityCode}"), CancellationToken.None))
                .ReturnsAsync(response);
        }


        private static async Task<Mock<IRestResponse>> CreateCurrentWeatherRestResponseMock()
        {

            var response = new Mock<IRestResponse>();
            var json = await LoadJsonResponse("Repositories/TestResponses/AccuWeatherCurrentWeather.json");
            response.Setup(x => x.Content).Returns(json);
            response.Setup(x => x.IsSuccessful).Returns(true);
            return response;
        }

        private static async Task<string> LoadJsonResponse(string path)
        {
            using var streamReader = new StreamReader(path);
            var result = await streamReader.ReadToEndAsync();
            return result;
        }

        [Fact]
        public async Task GetCurrentWeather_WithoutCityCode_CallProperMethod()
        {
            var clientMock = new Mock<IRestClient>();
            await SetupWeatherRestResponse(clientMock);
            await SetupCitySearchResponse(clientMock);
            var dateProvider = CreateDateProviderMock();
            var repository = AccuWeatherRepository.FromCityCode("", "", dateProvider.Object, clientMock.Object);

            await repository.GetCurrentWeather();

            Assert.Equal("1411530", repository.CityCode);
        }

        private static async Task<Mock<IRestResponse>> SetupCitySearchResponse(Mock<IRestClient> clientMock)
        {
            var response = await CreateCitySearchResponseMock();
            AddCitySearchResponseToRestClientMock(clientMock, response.Object);
            return response;
        }

        private static void AddCitySearchResponseToRestClientMock(Mock<IRestClient> clientMock, IRestResponse citySearchResponseMock)
        {
            clientMock.Setup(x =>
                x.ExecuteAsync(It.Is<RestRequest>(z => z.Resource == "locations/v1/cities/geoposition/search"),
                    CancellationToken.None)).ReturnsAsync(citySearchResponseMock);
        }

        private static async Task<Mock<IRestResponse>> CreateCitySearchResponseMock()
        {
            var response2 = new Mock<IRestResponse>();
            var responseResult = await LoadJsonResponse("Repositories/TestResponses/AccuWeatherCitySearch.json");
            response2.Setup(x => x.Content).Returns(responseResult);
            response2.Setup(x => x.IsSuccessful).Returns(true);
            return response2;
        }

        [Fact]
        public async Task GetCurrentWeather_JsonResponseIsEmpty_ThrowsNullReferenceException()
        {
            var client = new Mock<IRestClient>();
            var response = await SetupWeatherRestResponse(client);
            response.Setup(x => x.Content).Returns("");
            var dateProvider = CreateDateProviderMock();

            var repo = AccuWeatherRepository.FromCityCode("", _cityCode, dateProvider.Object, client.Object);

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => repo.GetCurrentWeather());
        }

        [Fact]
        public async Task GetCurrentWeather_HttpRequestWasNotSuccessful_ThrowsHttpRequestException()
        {
            var client = new Mock<IRestClient>();
            var response = await SetupWeatherRestResponse(client);
            response.Setup(x => x.IsSuccessful).Returns(false);
            var dateProvider = CreateDateProviderMock();

            var repo = AccuWeatherRepository.FromCityCode("", _cityCode, dateProvider.Object, client.Object);

            await Assert.ThrowsAnyAsync<HttpRequestException>(() => repo.GetCurrentWeather());
        }
    }
}