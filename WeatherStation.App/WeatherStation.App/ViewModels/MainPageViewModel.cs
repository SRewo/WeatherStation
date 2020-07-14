using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.App.ViewModels
{
    public class MainPageViewModel : BindableBase, INavigatedAware
    {
        private IBasicWeatherRepository _repository;
        private IContainsDailyForecast _dailyForecast;
        private IContainsHourlyForecast _hourlyForecast;
        private IDateProvider _dateProvider;

        private WeatherData _weatherData;

        public WeatherData WeatherData
        {
            get => _weatherData;
            set => SetProperty(ref _weatherData, value);
        }

        private IEnumerable<WeatherData> _weatherHourlyData;

        public IEnumerable<WeatherData> WeatherHourlyData
        {
            get => _weatherHourlyData;
            set => SetProperty(ref _weatherHourlyData, value);
        }

        public bool ContainsDailyForecasts { get; set; }
        public bool ContainsHourlyForecasts { get; set; }
        public DelegateCommand GetDataCommand { get; set; }


        private IEnumerable<WeatherData> _weatherDailyData;

        public MainPageViewModel(IBasicWeatherRepository repository, IContainsDailyForecast dailyForecast, IContainsHourlyForecast hourlyForecast, IDateProvider dateProvider)
        {
            _repository = repository ?? throw new NullReferenceException();
            _dailyForecast = dailyForecast;
            ContainsDailyForecasts = dailyForecast != null;
            _hourlyForecast = hourlyForecast;
            ContainsHourlyForecasts = hourlyForecast != null;
            _dateProvider = dateProvider;
            GetDataCommand = new DelegateCommand(async () =>
            {
                await GetData();
            });
        }

        public IEnumerable<WeatherData> WeatherDailyData
        {
            get => _weatherDailyData;
            set => SetProperty(ref _weatherDailyData, value);
        }

        public async Task GetData()
        {
            if (WeatherData == null || _dateProvider.GetActualDateTime() - WeatherData.Date > TimeSpan.FromMinutes(30))
            {
                WeatherData = await _repository.GetCurrentWeather();
                WeatherHourlyData = ContainsHourlyForecasts
                    ? await _hourlyForecast.GetHourlyForecast()
                    : new List<WeatherData>();
                WeatherDailyData = ContainsDailyForecasts
                    ? await _dailyForecast.GetDailyForecast()
                    : new List<WeatherData>();
            }
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            GetData();
        }
    }
}