using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IGeocodingRepository
    {
        Task<Coordinates> GetLocationCoordinates(string locationData);
    }
}
