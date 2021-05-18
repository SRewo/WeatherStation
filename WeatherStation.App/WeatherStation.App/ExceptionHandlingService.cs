using System;
using System.Threading.Tasks;

namespace WeatherStation.App{
    public interface IErrorHandlingService{
        Task HandleError(Exception ex);
    }

    public class ErrorHandlingService : IErrorHandlingService
    {
        private IErrorAlertService _alertService;

        public ErrorHandlingService(IErrorAlertService alertService)
        {
            _alertService = alertService;
        }
        public Task HandleError(Exception ex)
        {
            return _alertService.DisplayExceptionMessage(ex);
        }
    }
}