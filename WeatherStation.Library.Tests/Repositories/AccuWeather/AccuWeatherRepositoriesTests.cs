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
            var handlerMock = CreateRestRequestHandlerMock("Repositories/TestResponses/AccuWeatherCurrentWeather.json");
            var repository = new AccuWeatherCurrentWeatherRepository(handlerMock.Object, "", dateProviderMock.Object);

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.Single(weather);
        }

        private Mock<RestRequestHandler> CreateRestRequestHandlerMock(string jsonFileLocation)
        {
            var clientMock = new Mock<IRestClient>();
            var handlerMock = new Mock<RestRequestHandler>(clientMock.Object, "");
            var json = LoadJsonResponse(jsonFileLocation);
            handlerMock.Setup(x => x.GetDataFromApi(It.IsAny<IEnumerable<Parameter>>())).Returns(json);
            return handlerMock;
        }

        private static Mock<IDateProvider> CreateDateProviderMock()
        {
            var dateProviderMock = new Mock<IDateProvider>();
            dateProviderMock.Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01));
            return dateProviderMock;
        }

        private static async Task<dynamic> LoadJsonResponse(string path)
        {
            using var streamReader = new StreamReader(path);
            var result = await streamReader.ReadToEndAsync();
            return JsonConvert.DeserializeObject(result);
        }

        [Fact]
        public async Task HourlyForecastRepository_ProperExecution_ReturnsNotEmptyList()
        {
            var handlerMock = CreateRestRequestHandlerMock("Repositories/TestResponses/AccuWeatherHourlyForecasts.json");
            var repository = new AccuWeatherHourlyForecastRepository(handlerMock.Object, "");

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.NotEmpty(weather);
        }

        [Fact]
        public async Task DailyForecastsRepository_ProperExecution_ReturnsNotEmptyList()
        {
            var handlerMock = CreateRestRequestHandlerMock("Repositories/TestResponses/AccuWeatherDailyForecasts.json");
            var repository = new AccuWeatherDailyForecastRepository(handlerMock.Object, "");

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.NotEmpty(weather);
        }
    }
}