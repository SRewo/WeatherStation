using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherStation.App.Views
{

    public class MenuItem
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string TargetView { get; set; }

        public NavigationParameters Parameters { get; set; }
    }
}