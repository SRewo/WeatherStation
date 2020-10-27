using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherStation.Library.Interfaces
{
    public interface IWeatherRepository
    {
        string Language { get; set; }
        Task<IEnumerable<WeatherData>> GetWeatherDataFromRepository();
    }
}