﻿using System;

namespace WeatherStation.Library
{
    public class WeatherData
    {
        public DateTime Date { get; set; }
        public Temperature Temperature { get; set; }
        public Temperature TemperatureMin { get; set; }
        public Temperature TemperatureMax { get; set; }
        public Temperature TemperatureApparent { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public float WindSpeed { get; set; }
        public int WindDirection { get; set; }
        public int ChanceOfRain { get; set; }
        public float PrecipitationSummary { get; set; }
        public int WeatherCode { get; set; }
        public string WeatherDescription { get; set; }
    }
}