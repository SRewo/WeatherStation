using Moq;
using RestSharp;
using System.Linq;
using System.Threading.Tasks;
using WeatherStation.Library.Repositories.Weatherbit;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories.Weatherbit
{
    public class WeatherbitHourlyForecastRepositoryTests
    {
        private string _testResponseLocation = "Repositories/Weatherbit/TestResponses/hourlyForecasts.json";

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_ReturnsNotEmptyList()
        {
            var classes = await CreateTestClasses();

            var items = await classes.Item1.GetWeatherDataFromRepository();

            Assert.NotEmpty(items);
        }

        private async Task<(WeatherbitHourlyForecastRepository, Mock<IRestClient>)> CreateTestClasses()
        {
            var client = await CommonMethods.CreateRestClientMock(_testResponseLocation);
            var repository = new WeatherbitHourlyForecastRepository(client.Object,"", (0,0));
            repository.Language = "pl-PL";
            return (repository, client);
        }

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_ReturnsForecastFor24Hours()
        {
            var classes = await CreateTestClasses();

            var items = await classes.Item1.GetWeatherDataFromRepository();

            Assert.Equal(24, items.Count());
        }

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_DoesNotContaionDefaultWeatherDataObjects()
        {
            var classes = await CreateTestClasses();

            var items = await classes.Item1.GetWeatherDataFromRepository();

            Assert.DoesNotContain(new WeatherData(), items);
        }
    }
}
