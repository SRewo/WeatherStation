using System;
using System.Threading.Tasks;

namespace WeatherStation.App {
    public interface IErrorAlertService : IAlertService{
        Task DisplayExceptionMessage(Exception ex);
    }
    public class ErrorAlertService : AlertService, IErrorAlertService
    {
        public Task DisplayExceptionMessage (Exception ex){
            if(ex.Message != string.Empty)
                return DisplayAlert("Error", ex.Message + "\n For more info check logs.");
            else
                return DisplayAlert("Error", "Unknown error.\n For more info check logs.");
        }
    }
}