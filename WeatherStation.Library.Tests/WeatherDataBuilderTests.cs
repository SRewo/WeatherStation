using System;
using Xunit;

namespace WeatherStation.Library.Tests
{
    public class WeatherDataBuilderTests
    {
        [Fact]
        public void SetDate_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();
            var date = new DateTime(2020, 01, 01, 10, 11, 1);
            builder.SetDate(date);
            var data = builder.Build();

            Assert.Equal(date, data.Date);
        }

        [Theory]
        [InlineData(1899, 12, 31)]
        [InlineData(1707, 11, 23)]
        [InlineData(1500, 10, 20)]
        public void SetDate_DateEarlierThan1900_ThrowsException(int year, int month, int day)
        {
            var builder = new WeatherDataBuilder();
            var date = new DateTime(year, month, day);

            Assert.ThrowsAny<InvalidOperationException>(() => builder.SetDate(date));
        }

        [Theory]
        [InlineData(10, 10, TemperatureScale.Celsius)]
        [InlineData(-20, -20, TemperatureScale.Celsius)]
        public void SetTemperature_ProperlySetsValue(float input, float expectedValue, TemperatureScale unit)
        {
            var builder = new WeatherDataBuilder();
            builder.SetTemperature(input, unit);
            var data = builder.Build();

            Assert.Equal(expectedValue, data.Temperature.Value);
        }

        [Theory]
        [InlineData(10, 10, TemperatureScale.Celsius)]
        [InlineData(-20, -20, TemperatureScale.Celsius)]
        public void SetMinTemperature_ProperlySetsValue(float input, float expectedValue, TemperatureScale unit)
        {
            var builder = new WeatherDataBuilder();
            builder.SetMinTemperature(input, unit);
            var data = builder.Build();

            Assert.Equal(expectedValue, data.TemperatureMin.Value);
        }

        [Theory]
        [InlineData(10, 10, TemperatureScale.Celsius)]
        [InlineData(-20, -20, TemperatureScale.Celsius)]
        public void SetMaxTemperature_ProperlySetsValue(float input, float expectedValue, TemperatureScale unit)
        {
            var builder = new WeatherDataBuilder();
            builder.SetMaxTemperature(input, unit);
            var data = builder.Build();

            Assert.Equal(expectedValue, data.TemperatureMax.Value);
        }

        [Theory]
        [InlineData(10, 10, TemperatureScale.Celsius)]
        [InlineData(-20, -20, TemperatureScale.Celsius)]
        public void SetApparentTemperature_ProperlySetsValue(float input, float expectedValue, TemperatureScale unit)
        {
            var builder = new WeatherDataBuilder();
            builder.SetApparentTemperature(input, unit);
            var data = builder.Build();

            Assert.Equal(expectedValue, data.TemperatureApparent.Value);
        }

        [Fact]
        public void SetPressure_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();
            var pressure = 1010;
            builder.SetPressure(pressure);
            var data = builder.Build();

            Assert.Equal(pressure, data.Pressure);
        }

        [Theory]
        [InlineData(660)]
        [InlineData(899)]
        [InlineData(1101)]
        [InlineData(1540)]
        public void SetPressure_InvalidValue_ThrowsException(int value)
        {
            var builder = new WeatherDataBuilder();

            Assert.ThrowsAny<InvalidOperationException>(() => builder.SetPressure(value));
        }

        [Fact]
        public void SetHumidity_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();
            var humidity = 56;
            builder.SetHumidity(humidity);
            var data = builder.Build();

            Assert.Equal(humidity, data.Humidity);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(210)]
        [InlineData(-230)]
        public void SetHumidity_InvalidValue_ThrowsException(int value)
        {
            var builder = new WeatherDataBuilder();

            Assert.ThrowsAny<InvalidOperationException>(() => builder.SetHumidity(value));
        }

        [Fact]
        public void SetWindSpeed_KilometersPerHour_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();
            var windSpeed = 10;
            builder.SetWindSpeed(windSpeed, WindSpeedUnit.KilometersPerHour);

            var data = builder.Build();

            Assert.Equal(windSpeed, data.WindSpeed);
        }

        [Fact]
        public void SetWindSpeed_MetersPerSecond_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();

            var data = builder.SetWindSpeed(10,WindSpeedUnit.MetersPerSecond).Build();

            Assert.Equal(36, data.WindSpeed);
        }

        [Fact]
        public void SetWindSpeed_MilesPerHour_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();

            var data = builder.SetWindSpeed(10, WindSpeedUnit.MilesPerHour).Build();

            Assert.Equal(16.09f,data.WindSpeed);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(-0.1f)]
        [InlineData(-2030)]
        public void SetWindSpeed_InvalidValue_ThrowsException(float value)
        {
            var builder = new WeatherDataBuilder();

            Assert.ThrowsAny<InvalidOperationException>(() => builder.SetWindSpeed(value, WindSpeedUnit.KilometersPerHour));
        }

        [Fact]
        public void SetWindDirection_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();
            var windDirection = 110;
            builder.SetWindDirection(windDirection);
            var data = builder.Build();

            Assert.Equal(windDirection, data.WindDirection);
        }

        [Theory]
        [InlineData(361)]
        [InlineData(-1)]
        [InlineData(-32)]
        [InlineData(610)]
        public void SetWindDirection_InvalidValue_ThrowsException(int value)
        {
            var builder = new WeatherDataBuilder();

            Assert.ThrowsAny<InvalidOperationException>(() => builder.SetWindDirection(value));
        }

        [Fact]
        public void SetChanceOfRain_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();
            var chanceOfRain = 10;
            builder.SetChanceOfRain(chanceOfRain);
            var data = builder.Build();

            Assert.Equal(chanceOfRain, data.ChanceOfRain);
        }

        [Theory]
        [InlineData(101)]
        [InlineData(-1)]
        [InlineData(-230)]
        [InlineData(593)]
        public void SetChanceOfRain_InvalidValue_ThrowsException(int value)
        {
            var builder = new WeatherDataBuilder();

            Assert.ThrowsAny<InvalidOperationException>(() => builder.SetChanceOfRain(value));
        }

        [Fact]
        public void SetWeatherCode_ProperlySetsValue()
        {
            var builder = new WeatherDataBuilder();
            var weatherCode = 200;
            builder.SetWeatherCode(weatherCode);
            var data = builder.Build();

            Assert.Equal(weatherCode, data.WeatherCode);
        }
    }
}