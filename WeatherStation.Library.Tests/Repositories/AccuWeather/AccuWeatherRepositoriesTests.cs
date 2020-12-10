using System.Threading.Tasks;
using WeatherStation.Library.Repositories.AccuWeather;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories.AccuWeather
{
    public class AccuWeatherRepositoriesTests
    {

        [Fact]
        public async Task CurrentWeatherRepository_ProperExecution_ReturnsListWithSingleObject()
        {
            var dateProviderMock = CommonMethods.CreateDateProviderMock();
            var clientMock = await CommonMethods.CreateRestClientMock("Repositories/AccuWeather/TestResponses/AccuWeatherCurrentWeather.json");
            var repository = new AccuWeatherCurrentWeatherRepository(clientMock.Object,"", "", dateProviderMock.Object);

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.Single(weather);
        }

        
        [Fact]
        public async Task HourlyForecastRepository_ProperExecution_ReturnsNotEmptyList()
        {
            var clientMock = await CommonMethods.CreateRestClientMock("Repositories/AccuWeather/TestResponses/AccuWeatherHourlyForecasts.json");
            var repository = new AccuWeatherHourlyForecastRepository(clientMock.Object, "", "");

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.NotEmpty(weather);
        }

        [Fact]
        public async Task DailyForecastsRepository_ProperExecution_ReturnsNotEmptyList()
        {
            var clientMock = await CommonMethods.CreateRestClientMock("Repositories/AccuWeather/TestResponses/AccuWeatherDailyForecasts.json");
            var repository = new AccuWeatherDailyForecastRepository(clientMock.Object, "", "");

            var weather = await repository.GetWeatherDataFromRepository();

            Assert.NotEmpty(weather);
        }
    }
}