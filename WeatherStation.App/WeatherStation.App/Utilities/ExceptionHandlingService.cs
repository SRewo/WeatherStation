using System.Text;
using System;
using System.Threading.Tasks;

namespace WeatherStation.App.Utilities{
    public interface IExceptionHandlingService{
        Task HandleException(Exception ex);
    }

    public class ExceptionHandlingService : IExceptionHandlingService
    {
        private IErrorAlertService _alertService;
        private ILogger _logger;

        public ExceptionHandlingService(IErrorAlertService alertService, ILogger logger)
        {
            _alertService = alertService;
            _logger = logger;
        }
        public Task HandleException(Exception ex)
        {
            if(ex is null)
                return HandleNull();
            else
                return HandleExceptionObject(ex);
        }

        private Task HandleNull()
        {
            _logger.Log("Unknown Error \n");
            return _alertService.DisplayAlert("Unknown Error", "Unknown Error");
        }

        private Task HandleExceptionObject(Exception ex)
        {
            _logger.Log(ex.ToString() + "\n");
            return _alertService.DisplayExceptionMessage(ex);
        }

    }
}