using System;

namespace WeatherStation.Library
{
    public class InvalidCoordinatesException : Exception
    {
        public InvalidCoordinatesException(string message) : base(message)
        {
        }

        public InvalidCoordinatesException() : base()
        {
        }

        public InvalidCoordinatesException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class NoLocationDataException : Exception
    {
        public NoLocationDataException(string message) : base(message) 
        {
        }

        public NoLocationDataException() : base()
        {
        }

        public NoLocationDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
