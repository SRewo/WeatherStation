using System;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Prism.Commands;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
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
        private IGeocoding _geocoding;
        private IAlertService _alertService;
        private float _latitude;
        private float _longitude;
        private string _cityName;

        public string CityName
        {
            get => _cityName;
            set
            {
                SetProperty(ref _cityName, value);
                AreCoordinatesUsed = false;
            }
        }

        public DelegateCommand SaveSettingsCommand { get; set; }
        public DelegateCommand GetLocationCommand { get; set; }
        public bool AreCoordinatesUsed { get; private set; } = false;

        public SettingsViewModel(IEnumerable<IWeatherRepositoryStore> weatherRepositoryStores, IPreferences preferences, IGeolocation geolocation, IAlertService alertService, IGeocoding geocoding)
        {
            _weatherRepositoryStores = weatherRepositoryStores;
            _preferences = preferences;
            _geolocation = geolocation;
            _alertService = alertService;
            _geocoding = geocoding;
            CityName = preferences.Get("CityName", "");
            SaveSettingsCommand = new DelegateCommand(async () => await SaveSettings());
            GetLocationCommand = new DelegateCommand(async () => await GetLocation());
        }

        public async Task SaveSettings()
        {
            try
            {
                await Save();
            }
            catch (AccuWeatherCityDataFromCityName.InvalidCityNameException ex)
            {
                await _alertService.DisplayAlert("Invalid city name", ex.Message);
            }
            catch (AccuWeatherCityDataFromCityName.MultipleCitiesResultException ex)
            {
                await _alertService.DisplayAlert("City search returned multiple results", ex.Message);
            }
        }

        private async Task Save()
        {
            var currentCity = _preferences.Get("CityName", "");
            if(CityName != currentCity)
                await SaveCitySettings();
            await _alertService.DisplayAlert("Settings Saved", "Settings was saved.");
        }

        private async Task SaveCitySettings()
        {
            foreach (var repositoryStore in _weatherRepositoryStores)
                await SaveCitySettingsInRepositoryStore(repositoryStore);
            _preferences.Set("CityName", CityName);
        }

        private async Task SaveCitySettingsInRepositoryStore(IWeatherRepositoryStore store)
        {
            if (AreCoordinatesUsed)
                await store.ChangeCity(_latitude, _longitude);
            else
                await store.ChangeCity(CityName);
            _preferences.Set($"{store.RepositoryName}CityId", store.CityId);
        }

        public async Task GetLocation()
        {
            try
            {
                await GetAndSaveLocation();
            }
            catch (PermissionException ex)
            {
                await _alertService.DisplayAlert("Permissions problem", ex.Message);
            }
            catch (Exception ex)
            {
                await _alertService.DisplayAlert("Error", ex.Message);
            }

        }

        private async Task GetAndSaveLocation()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium);
            var location = await _geolocation.GetLocationAsync(request);
            if (location == null)
                await _alertService.DisplayAlert("Error", "Location search result was null");
            else
                await GetGeolocationDataFromResult(location);
        }

        private async Task GetGeolocationDataFromResult(Location location)
        {
            await GetCoordinatesFromResult(location);
            await GetCityDataFromResult(location);
            AreCoordinatesUsed = true;
        }

        private Task GetCoordinatesFromResult(Location location)
        {
            _latitude = (float) location.Latitude;
            _longitude = (float) location.Longitude;
            return Task.CompletedTask;
        }

        private async Task GetCityDataFromResult(Location location)
        {
            var placemarks = await _geocoding.GetPlacemarksAsync(location);
            var locationData = placemarks.FirstOrDefault();
            if(locationData != null)
                CityName = $"{locationData.Locality},{locationData.CountryName}";
        }


    }
}
