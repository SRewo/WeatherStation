using System;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Library
{
    public class DateProvider : IDateProvider
    {
        public DateTime GetActualDateTime() => DateTime.Now;
    }
}