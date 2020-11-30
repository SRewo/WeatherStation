using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using WeatherStation.Library.Repositories.AccuWeather;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories.AccuWeather
{
    public class AccuWeatherRepositoriesTests
    {

        [Fact]
        public async Task CurrentWeatherRepository_ProperExecution_ReturnsListWithSingleObject()
        {
            var dateProviderMock = CreateDateProviderMock();
            var clientMock = await CreateRestClientMock("Repositories/TestResponses/AccuWeatherCurrentWeather.json");
            var repository = new AccuWeatherCurrentWeatherRepository(clientMock.Object,"", "", dateProviderMock.Object);

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.Single(weather);
        }

        private async Task<Mock<IRestClient>> CreateRestClientMock(string jsonFileLocation)
        {
            var clientMock = new Mock<IRestClient>();
            var responseMock = await CreateResponseMock(jsonFileLocation);
            clientMock.Setup(x => x.ExecuteAsync(It.IsAny<IRestRequest>(), CancellationToken.None)).ReturnsAsync(responseMock.Object);
            return clientMock;
        }

        private static Mock<IDateProvider> CreateDateProviderMock()
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

        [Fact]
        public async Task HourlyForecastRepository_ProperExecution_ReturnsNotEmptyList()
        {
            var clientMock = await CreateRestClientMock("Repositories/TestResponses/AccuWeatherHourlyForecasts.json");
            var repository = new AccuWeatherHourlyForecastRepository(clientMock.Object, "", "");

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.NotEmpty(weather);
        }

        [Fact]
        public async Task DailyForecastsRepository_ProperExecution_ReturnsNotEmptyList()
        {
            var clientMock = await CreateRestClientMock("Repositories/TestResponses/AccuWeatherDailyForecasts.json");
            var repository = new AccuWeatherDailyForecastRepository(clientMock.Object, "", "");

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.NotEmpty(weather);
        }
    }
}