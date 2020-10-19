using System;
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
        private readonly IDateProvider _dateProvider;
        private IWeatherRepository _repository;


        private IEnumerable<WeatherData> _weatherDailyData;

        private WeatherData _weatherData;

        private IEnumerable<WeatherData> _weatherHourlyData;

        public MainPageViewModel(IDateProvider dateProvider)
        {
            _dateProvider = dateProvider;
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
            _repository = (IWeatherRepository) parameters["repository"];
            await CheckIfRepositoryContainsDailyAndHourlyForecasts();
            await GetData();
        }

        public async Task GetData()
        {
            try
            {
                await GetDataIfNotCurrent();
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
            }
        }

        public async Task GetDataIfNotCurrent()
        {
            if (!IsWeatherDataCurrent())
                await DownloadWeatherDataFromRepository();
        }

        public Task CheckIfRepositoryContainsDailyAndHourlyForecasts()
        {
            ContainsDailyForecasts = _repository.DailyRepository != null;
            ContainsHourlyForecasts = _repository.HourlyRepository != null;
            return Task.CompletedTask;
        }

        public async Task DownloadWeatherDataFromRepository()
        {
            WeatherData = await _repository.GetCurrentWeather();
            //WeatherHourlyData = ContainsHourlyForecasts
                //  ? await _repository.HourlyRepository.GetHourlyForecast()
                //  : new List<WeatherData>();
            //WeatherDailyData = ContainsDailyForecasts
                //   ? await _repository.DailyRepository.GetDailyForecast()
                //   : new List<WeatherData>();
        }

        public bool IsWeatherDataCurrent()
        {
            try
            {
                return _dateProvider.GetActualDateTime() - WeatherData.Date <= TimeSpan.FromHours(1);
            }
            catch
            {
                return false;
            }
        }
    }
}