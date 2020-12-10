using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IGeocodingRepository
    {
        Task<(double,double)> GetLocationCoordinates(string locationData);
    }
}
