using System;

namespace WeatherStation.Library.Interfaces
{
    public interface IDateProvider
    {
        DateTime GetActualDateTime();
    }
}