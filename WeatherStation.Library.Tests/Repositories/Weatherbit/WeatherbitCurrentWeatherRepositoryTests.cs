using Moq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeatherStation.Library.Repositories.Weatherbit;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories.Weatherbit
{
    public class WeatherbitCurrentWeatherRepositoryTests
    {
       private string currentWeatherJsonPath = "Repositories/Weatherbit/TestResponses/currentWeather.json";

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_ReturnsWeatherDataCollection()
        {
            var repository = await CreateDefaultRepository();

            var result = await repository.Item1.GetWeatherDataFromRepository();

            _ = Assert.IsAssignableFrom<IEnumerable<WeatherData>>(result);
        }

        private async Task<(WeatherbitCurrentWeatherRepository, Mock<IRestClient>)> CreateDefaultRepository()
        {
            var client = await CommonMethods.CreateRestClientMock(currentWeatherJsonPath);
            var dateProvider = CommonMethods.CreateDateProviderMock();
            var repository = new WeatherbitCurrentWeatherRepository(client.Object, "",new Coordinates(0,0), dateProvider.Object);
            repository.Language = "pl-PL";
            return (repository, client);
        }

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_ReturnsSingleItem()
        {
            var repository = await CreateDefaultRepository();

            var result = await repository.Item1.GetWeatherDataFromRepository();

            Assert.Single(result);
        }
        
        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_HasProperLanguageCodeInRequest()
        {
            var repository = await CreateDefaultRepository();

            var result = await repository.Item1.GetWeatherDataFromRepository();

            repository.Item2.Verify(x => x.ExecuteAsync(
                It.Is<IRestRequest>(x => x.Parameters.Any(x => x.Name == "lang" && x.Value.ToString() == "pl")), 
                CancellationToken.None));
        }

        [Fact]
        public async Task GetWeatherDataFromRepository_ValidExecution_HasProperDateTime()
        {
            var repository = await CreateDefaultRepository();

            var result = await repository.Item1.GetWeatherDataFromRepository();

            Assert.Equal(new DateTime(2020,01,01), result.FirstOrDefault().Date);
        }

    }
}
