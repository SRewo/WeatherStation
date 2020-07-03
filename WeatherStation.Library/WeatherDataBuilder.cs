using System;

namespace WeatherStation.Library
{
    public enum TemperatureUnit
    {
        Celcius,
        Kelvin,
        Fahrenheit
    }

    public sealed class WeatherDataBuilder : FunctionalBuilder<WeatherData, WeatherDataBuilder>
    {
        public WeatherDataBuilder SetDate(DateTime date)
        {
            if (date < new DateTime(1900, 01, 01))
                throw new InvalidOperationException("Provided date cannot be earlier than 1900.01.01");

            return Do(x => x.Date = date);
        }

        public WeatherDataBuilder SetTemperature(float value, TemperatureUnit unit)
        {
            return Do(x => x.Temperature = ChangeToCelcius(value, unit));
        }

        public WeatherDataBuilder SetMinTemperature(float value, TemperatureUnit unit)
        {
            return Do(x => x.TemperatureMin = ChangeToCelcius(value, unit));
        }

        public WeatherDataBuilder SetMaxTemperature(float value, TemperatureUnit unit)
        {
            return Do(x => x.TemperatureMax = ChangeToCelcius(value, unit));
        }

        public WeatherDataBuilder SetApparentTemperature(float value, TemperatureUnit unit)
        {
            return Do(x => x.TemperatureApparent = ChangeToCelcius(value, unit));
        }

        public WeatherDataBuilder SetPressure(int value)
        {
            if (value < 900 || value > 1100)
                throw new InvalidOperationException("Pressure value cannot be smaller than 900 or grater than 1100");

            return Do(x => x.Pressure = value);
        }

        public WeatherDataBuilder SetHumidity(int value)
        {
            if (value < 0 || value > 100)
                throw new InvalidOperationException("Humidity value cannot be smaller than 0 or grater than 100");

            return Do(x => x.Humidity = value);
        }

        public WeatherDataBuilder SetWindSpeed(float value)
        {
            if (value < 0)
                throw new InvalidOperationException("Wind speed cannot be smaller than 0");

            return Do(x => x.WindSpeed = value);
        }

        public WeatherDataBuilder SetWindDirection(int value)
        {
            if (value < 0 || value > 360)
                throw new InvalidOperationException("Wind direction cannot be smaller than 0 or grater than 360");

            return Do(x => x.WindDirection = value);
        }

        public WeatherDataBuilder SetChanceOfRain(int value)
        {
            if (value < 0 || value > 100)
                throw new InvalidOperationException("Chance of rain cannot be smaller than 0 or grater than 100");

            return Do(x => x.ChanceOfRain = value);
        }

        public WeatherDataBuilder SetPrecipitationSummary(float value)
        {
            if (value < 0)
                throw new InvalidOperationException("Precipitation summary cannot be smaller than 0");

            return Do(x => x.PrecipitationSummary = value);
        }

        public WeatherDataBuilder SetWeatherCode(int value)
        {
            return Do(x => x.WeatherCode = value);
        }

        public WeatherDataBuilder SetWeatherDescription(string value)
        {
            if(value == string.Empty)
                throw new NullReferenceException("Description cannot be empty");

            return Do(x => x.WeatherDescription = value);
        }


        private float ChangeToCelcius(float value, TemperatureUnit unit)
        {
            switch (unit)
            {
                case TemperatureUnit.Fahrenheit:
                    return (value - 32) / 1.8f;
                case TemperatureUnit.Kelvin:
                    return value - 273.15f;
                case TemperatureUnit.Celcius:
                    return value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
            }
        }
    }
}