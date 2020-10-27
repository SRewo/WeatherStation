using Prism.Navigation;

namespace WeatherStation.App
{

    public class MenuItem
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string TargetView { get; set; }

        public NavigationParameters Parameters { get; set; }
    }
}