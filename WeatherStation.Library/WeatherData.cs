using System;

namespace WeatherStation.Library
{
    public class WeatherData
    {
        public DateTime Date { get; set; }
        public float Temperature { get; set; }
        public float TemperatureMin { get; set; }
        public float TemperatureMax { get; set; }
        public float TemperatureApparent { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public float WindSpeed { get; set; }
        public int WindDirection { get; set; }
        public int ChanceOfRain { get; set; }
        public int WeatherCode { get; set; }
    }
}