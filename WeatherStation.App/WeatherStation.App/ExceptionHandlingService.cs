using System;
using System.Threading.Tasks;

namespace WeatherStation.App{
    public interface IExceptionHandlingService{
        Task HandleException(Exception ex);
    }

    public class ExceptionHandlingService : IExceptionHandlingService
    {
        private IErrorAlertService _alertService;

        public ExceptionHandlingService(IErrorAlertService alertService)
        {
            _alertService = alertService;
        }
        public Task HandleException(Exception ex)
        {
            return _alertService.DisplayExceptionMessage(ex);
        }
    }
}