using Xunit;

namespace WeatherStation.Library.Tests
{
    public class CoordinatesTests
    {
        [Theory]
        [InlineData(-91,89)]
        [InlineData(91,89)]
        [InlineData(22,-181)]
        [InlineData(22,181)]
        public void IsValid_CoordinatesArentValid_ReturnsFalse(double lat, double lon)
        {
            var coordinates = new Coordinates(lat, lon);

            var result = coordinates.IsValid();

            Assert.False(result);
        }

        [Theory]
        [InlineData(90,20)]
        [InlineData(-90,20)]
        [InlineData(20,180)]
        [InlineData(20,-180)]
        [InlineData(90,180)]
        [InlineData(20,20)]
        public void IsValid_CoordinatesAreValid_ReturnsTrue(double lat, double lon) 
        {
            var coordinates = new Coordinates(lat,lon);
            
            var result = coordinates.IsValid();

            Assert.True(result);
        }
    }
}
