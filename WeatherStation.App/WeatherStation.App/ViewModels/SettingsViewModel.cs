using System;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using WeatherStation.Library.Interfaces;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using WeatherStation.Library;

namespace WeatherStation.App.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private IEnumerable<IWeatherRepositoryStore> _weatherRepositoryStores;
        private IPreferences _preferences;
        private IGeolocation _geolocation;
        private IGeocoding _geocoding;
        private IGeocodingRepository _geocodingRepository;
        private IAlertService _alertService;
        private IExceptionHandlingService _handlingService;
        private double _latitude;
        private double _longitude;
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

        public SettingsViewModel(IEnumerable<IWeatherRepositoryStore> weatherRepositoryStores, IPreferences preferences, IGeolocation geolocation, IAlertService alertService, IGeocoding geocoding, IGeocodingRepository geocodingRepository, IExceptionHandlingService service)
        {
            _handlingService = service;
            _geocodingRepository = geocodingRepository;
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
            catch (Exception ex)
            {
                await _handlingService.HandleException(ex);
            }
        }

        private async Task Save()
        {
            var currentCity = _preferences.Get("CityName", "");
            if(CityName != currentCity)
                await SaveCitySettings();
            
            await _alertService.DisplayAlert("Settings Saved", "Settings was saved.");
            SaveCoordinatesInPreferences();
        }

        private void SaveCoordinatesInPreferences()
        {
            Preferences.Set("lat", _latitude);
            Preferences.Set("lon", _longitude);
        }

        private async Task SaveCitySettings()
        {
            if(!AreCoordinatesUsed)
                await GetCoordinatesFromRepository();

            foreach (var repositoryStore in _weatherRepositoryStores)
                await SaveCitySettingsInRepositoryStore(repositoryStore);
            _preferences.Set("CityName", CityName);
        }

        private async Task GetCoordinatesFromRepository()
        {
            var apiResult =  await _geocodingRepository.GetLocationCoordinates(CityName);
            _latitude = apiResult.Latitude;
            _longitude = apiResult.Longitude;
        }

        private async Task SaveCitySettingsInRepositoryStore(IWeatherRepositoryStore store)
        {
             await store.ChangeCity(new Coordinates(_latitude, _longitude));

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
                await _handlingService.HandleException(ex);
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

            SaveCoordinatesInPreferences();
        }

        private async Task GetGeolocationDataFromResult(Location location)
        {
            await GetCoordinatesFromResult(location);
            await GetCityDataFromResult(location);
            AreCoordinatesUsed = true;
        }

        private Task GetCoordinatesFromResult(Location location)
        {
            _latitude = location.Latitude;
            _longitude = location.Longitude;
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
