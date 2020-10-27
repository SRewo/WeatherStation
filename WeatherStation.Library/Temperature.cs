using System;

namespace WeatherStation.Library
{
    public enum TemperatureScale
    {
        Celsius,
        Fahrenheit
    }

    public abstract class Temperature
    {
        protected string _unitLetters;
        public float Value { get; set; }
        public TemperatureScale Scale { get; protected set; }

        public override string ToString()
        {
            return $"{Value} {_unitLetters}";
        }
    }

    public class CelsiusTemperature : Temperature
    {
        public CelsiusTemperature()
        {
            Scale = TemperatureScale.Celsius;
            _unitLetters = "°C";
        }

        public CelsiusTemperature(float value) : this()
        {
            Value = value;
        }
    }

    public class FahrenheitTemperature : Temperature
    {
        public FahrenheitTemperature()
        {
            Scale = TemperatureScale.Fahrenheit;
            _unitLetters = "°F";
        }

        public FahrenheitTemperature(float value) : this()
        {
            Value = value;
        }
    }
}