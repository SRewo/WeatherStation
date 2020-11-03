using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Prism;
using Prism.Commands;
using WeatherStation.Library.Interfaces;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace WeatherStation.App.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private IEnumerable<IWeatherRepositoryStore> _weatherRepositoryStores;
        private IPreferences _preferences;
        private IGeolocation _geolocation;
        private IAlertService _alertService;
        private string _cityName;

        public string CityName
        {
            get => _cityName;
            set => SetProperty(ref _cityName, value);
        }

        public DelegateCommand SaveSettingsCommand { get; set; }

        public SettingsViewModel(IEnumerable<IWeatherRepositoryStore> weatherRepositoryStores, IPreferences preferences, IGeolocation geolocation, IAlertService alertService)
        {
            _weatherRepositoryStores = weatherRepositoryStores;
            _preferences = preferences;
            _geolocation = geolocation;
            _alertService = alertService;
            CityName = preferences.Get("CityName", "");
            SaveSettingsCommand = new DelegateCommand(async () => await SaveSettings());
        }

        public async Task SaveSettings()
        {
        }

    }
}
