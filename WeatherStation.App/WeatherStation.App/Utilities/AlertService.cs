using System.Threading.Tasks;

namespace WeatherStation.App.Utilities
{
    public interface IAlertService
    {
        Task DisplayAlert(string title, string message);
    }
    public class AlertService : IAlertService
    {
        public Task DisplayAlert(string title, string message)
        {
            return App.Current.MainPage.DisplayAlert(title, message, "Ok");
        }
    }
}