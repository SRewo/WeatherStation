using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Moq;
using RestSharp;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using Xunit;

namespace WeatherStation.Library.Tests.Repositories
{
    public class AccuWeatherRepositoryTests
    {
        [Fact]
        public async Task GetCurrentWeather_WorksProperly()
        {
            var mock = AutoMock.GetLoose();
            var response = new Mock<IRestResponse>();
            using var r = new StreamReader("Repositories/TestResponses/AccuWeatherCurrentWeather.json");
            var json = await r.ReadToEndAsync();
            response.Setup(x => x.Content).Returns(json);
            response.Setup(x => x.IsSuccessful).Returns(true);
            mock.Mock<IRestClient>().Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), CancellationToken.None))
                .ReturnsAsync(response.Object);
            mock.Mock<IDateProvider>().Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01));
            var repo = AccuWeatherRepository.CreateInstanceWithCityCode("", "1234",
                mock.Container.Resolve<IDateProvider>(), mock.Container.Resolve<IRestClient>());

            var weather = await repo.GetCurrentWeather();

            Assert.NotNull(weather);
            Assert.Equal(63, weather.Humidity);
            Assert.Equal(new DateTime(2020,01,01), weather.Date);
        }

        [Fact]
        public async Task GetCurrentWeather_WithoutCityCode_CallProperMethod()
        {
            var mock = AutoMock.GetLoose();
            var response = new Mock<IRestResponse>();
            var response2 = new Mock<IRestResponse>();
            using var r = new StreamReader("Repositories/TestResponses/AccuWeatherCurrentWeather.json");
            var json = await r.ReadToEndAsync();
            response.Setup(x => x.Content).Returns(json);
            response.Setup(x => x.IsSuccessful).Returns(true);
            response2.Setup(x => x.Content).Returns("{Key: '1234'}");
            response2.Setup(x => x.IsSuccessful).Returns(true);
            mock.Mock<IRestClient>().Setup(x =>
                    x.ExecuteAsync(It.Is<RestRequest>(z => z.Resource == "currentconditions/v1/1234"),
                        CancellationToken.None))
                .ReturnsAsync(response.Object);
            mock.Mock<IRestClient>().Setup(x =>
                x.ExecuteAsync(It.Is<RestRequest>(z => z.Resource == "locations/v1/cities/geoposition/search"),
                    CancellationToken.None)).ReturnsAsync(response2.Object);
            mock.Mock<IDateProvider>().Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01));
            var repo = AccuWeatherRepository.CreateInstanceWithCityCode("", "", mock.Create<IDateProvider>(),
                mock.Create<IRestClient>());

            await repo.GetCurrentWeather();

            Assert.Equal("1234", repo.CityCode);
        }

        [Fact]
        public async Task GetCurrentWeather_JsonIsEmpty_ThrowsNullReferenceException()
        {
            var mock = AutoMock.GetLoose();
            var response = new Mock<IRestResponse>();
            response.Setup(x => x.Content).Returns("");
            response.Setup(x => x.IsSuccessful).Returns(true);
            mock.Mock<IRestClient>().Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), CancellationToken.None))
                .ReturnsAsync(response.Object);
            mock.Mock<IDateProvider>().Setup(x => x.GetActualDateTime()).Returns(new DateTime(2020, 01, 01));

            var repo = AccuWeatherRepository.CreateInstanceWithCityCode("", "1234",
                mock.Container.Resolve<IDateProvider>(),
                mock.Container.Resolve<IRestClient>());

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => repo.GetCurrentWeather());
        }
    }
}