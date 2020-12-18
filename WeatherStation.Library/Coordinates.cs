using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherStation.Library
{
    public class Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Coordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public virtual bool IsValid()
        {
            return !IsLatitudeOutOfRange() && !IsLongitudeOutOfRange();
        }

        private bool IsLatitudeOutOfRange()
        {
            return Latitude < -90 || Latitude > 90;
        }

        private bool IsLongitudeOutOfRange()
        {
            return Longitude < -180 || Longitude > 180;
        }
    }
}
