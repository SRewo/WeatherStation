namespace WeatherStation.Library.Interfaces
{
    public interface IWeatherRepository : IBasicWeatherRepository
    {
        IContainsDailyForecast DailyRepository { get; }
        IContainsHistoricalData HistoricalRepository { get; }
        IContainsHourlyForecast HourlyRepository { get; }
    }
}