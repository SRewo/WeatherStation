using System;
using System.Threading.Tasks;
using Xunit;
using static WeatherStation.Services.Weather;

namespace WeatherStation.Services.Tests.Integration
{
    public class WeatherServiceTests : IClassFixture<WeatherServiceTestFixture>
    {
        private WeatherServiceTestFixture _fixture;
        private WeatherClient _client;

        public WeatherServiceTests(WeatherServiceTestFixture fixture)
        {
            _fixture = fixture;
            _client = new WeatherClient(fixture.GrpcChannel);
        }

        [Theory]
        [InlineData(Repositories.Accuweather)]
        [InlineData(Repositories.Weatherbit)]
        [InlineData(Repositories.Openweathermap)]
        public async Task CurrentWeather_ValidCall(Repositories repository)
        {
            var request = new WeatherRequest()
            {
                Repository = repository 
            };

            var result = await _client.GetCurrentWeatherAsync(request);

            Assert.NotNull(result);
            Assert.NotNull(result.Weather);
        }

        [Theory]
        [InlineData(Repositories.Accuweather)]
        [InlineData(Repositories.Weatherbit)]
        [InlineData(Repositories.Openweathermap)]
        public async Task DailyForecasts_ValidCall(Repositories repositories)
        {
            var request = new WeatherRequest()
            {
                Repository = repositories
            };

            var result = await _client.GetDailyForecastsAsync(request);

            Assert.NotNull(result);
            Assert.NotNull(result.Forecasts);
        }

        [Theory]
        [InlineData(Repositories.Accuweather)]
        [InlineData(Repositories.Openweathermap)]
        public async Task HourlyForecasts_ValidCall(Repositories repositories)
        {
            var request = new WeatherRequest()
            {
                Repository = repositories
            };

            var result = await _client.GetHourlyForecastsAsync(request);

            Assert.NotNull(result);
            Assert.NotNull(result.Forecasts);
        }
    }
}
