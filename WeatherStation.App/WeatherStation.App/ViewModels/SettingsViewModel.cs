using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Prism;
using Prism.Commands;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories.AccuWeather;
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
            try
            {
                await Save();
            }
            catch (InvalidCityNameException ex)
            {
                await _alertService.DisplayAlert("Invalid city name", ex.Message);
            }
            catch (MultipleCitiesResultException ex)
            {
                await _alertService.DisplayAlert("City search returned multiple results", ex.Message);
            }
        }

        public async Task Save()
        {
            var currentCity = _preferences.Get("CityName", "");
            if(CityName != currentCity)
                await SaveCitySettings();
            await _alertService.DisplayAlert("Settings Saved", "Settings was saved.");
        }

        public async Task SaveCitySettings()
        {
            foreach (var repositoryStore in _weatherRepositoryStores)
                await SaveCitySettingsInRepositoryStore(repositoryStore);
            _preferences.Set("CityName", CityName);
        }

        public async Task SaveCitySettingsInRepositoryStore(IWeatherRepositoryStore store)
        {
            await store.ChangeCity(CityName);
            _preferences.Set($"{store.RepositoryName}CityId", store.CityId);
        }

    }
}
