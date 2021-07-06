using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherStation.Library
{
    public class Coordinates
    {
        protected bool Equals(Coordinates other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coordinates) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }

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
