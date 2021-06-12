using System.Threading.Tasks;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.App.Utilities
{
	public interface ILogger
	{
		Task Log(string message);
	}

	public class Logger : ILogger
	{
		private IWriteService _service;
		private IDateProvider _dateProvider;

		public Logger(IWriteService writeService, IDateProvider dateProvider)
		{
			_service = writeService;
			_dateProvider = dateProvider;
		}

		public async Task Log(string message)
		{
			string date = _dateProvider.GetActualDateTime().ToString("dd.MM.yyyy hh:mm:ss");
			await _service.WriteFromString($"{date}: {message}\n");
		}
	}
}