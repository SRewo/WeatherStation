using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using Xamarin.Forms.Internals;

namespace WeatherStation.App.ViewModels
{
    public class MainPageViewModel : BindableBase, INavigatedAware
    {
        private readonly IDateProvider _dateProvider;
        private readonly IWeatherRepository _repository;


        private IEnumerable<WeatherData> _weatherDailyData;

        private WeatherData _weatherData;

        private IEnumerable<WeatherData> _weatherHourlyData;

        public MainPageViewModel(IDateProvider dateProvider, IWeatherRepository repository)
        {
            ContainsDailyForecasts = repository.DailyRepository != null;
            ContainsHourlyForecasts = repository.DailyRepository != null;
            _dateProvider = dateProvider;
            _repository = repository;
            GetDataCommand = new DelegateCommand(async () => { await GetData(); });
        }

        public WeatherData WeatherData
        {
            get => _weatherData;
            set => SetProperty(ref _weatherData, value);
        }

        public IEnumerable<WeatherData> WeatherHourlyData
        {
            get => _weatherHourlyData;
            set => SetProperty(ref _weatherHourlyData, value);
        }

        public bool ContainsDailyForecasts { get; set; }
        public bool ContainsHourlyForecasts { get; set; }
        public DelegateCommand GetDataCommand { get; set; }

        public IEnumerable<WeatherData> WeatherDailyData
        {
            get => _weatherDailyData;
            set => SetProperty(ref _weatherDailyData, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await GetData();
        }

        public async Task GetData()
        {
            if (WeatherData == null || _dateProvider.GetActualDateTime() - WeatherData.Date > TimeSpan.FromMinutes(30))
            {
                WeatherData = await _repository.GetCurrentWeather();
                //WeatherHourlyData = ContainsHourlyForecasts
                  //  ? await _repository.HourlyRepository.GetHourlyForecast()
                  //  : new List<WeatherData>();
                //WeatherDailyData = ContainsDailyForecasts
                 //   ? await _repository.DailyRepository.GetDailyForecast()
                 //   : new List<WeatherData>();
            }
        }
    }
}