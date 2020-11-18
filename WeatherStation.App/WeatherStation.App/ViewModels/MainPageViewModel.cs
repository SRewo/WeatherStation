using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Intents;
using Microcharts;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SkiaSharp;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using Xamarin.Essentials.Interfaces;

namespace WeatherStation.App.ViewModels
{
    public class MainPageViewModel : BindableBase, INavigatedAware
    {
        protected IDateProvider DateProvider;
        private IWeatherRepositoryStore _repositoryStore;
        private IPreferences _preferences;


        private IEnumerable<WeatherData> _weatherDailyData;

        private WeatherData _weatherData;

        private IEnumerable<WeatherData> _weatherHourlyData;

        private Chart _chart;

        public MainPageViewModel(IDateProvider dateProvider, IPreferences preferences)
        {
            DateProvider = dateProvider;
            _preferences = preferences;
            GetDataCommand = new DelegateCommand(async () => { await GetData(); });
            CityName = _preferences.Get("CityName", "");
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

        public Chart Chart
        {
            get => _chart;
            set => SetProperty(ref _chart, value);
        }

        private string _cityName;

        public string CityName
        {
            get => _cityName;
            set => SetProperty(ref _cityName, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await PerformRequiredTasks(parameters);
        }

        public async Task PerformRequiredTasks(INavigationParameters parameters)
        {
            await GetVariablesFromParameters(parameters);
            await CheckIfRepositoryContainsDailyAndHourlyForecasts();
            await GetData();
            await CreateChart();
        }

        private Task GetVariablesFromParameters(INavigationParameters parameters)
        {
            _repositoryStore = (IWeatherRepositoryStore) parameters["repositoryStore"];
            return Task.CompletedTask;
        }

        private async Task GetData()
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

        private async Task GetDataIfNotCurrent()
        {
            if (!IsWeatherDataCurrent())
                await DownloadWeatherDataFromRepository();
        }

        private Task CheckIfRepositoryContainsDailyAndHourlyForecasts()
        {
            ContainsDailyForecasts = _repositoryStore.DailyForecastsRepository != null;
            ContainsHourlyForecasts = _repositoryStore.HourlyForecastsRepository != null;
            return Task.CompletedTask;
        }

        private async Task DownloadWeatherDataFromRepository()
        {
            var currentWeather = await _repositoryStore.CurrentWeatherRepository.GetWeatherDataFromRepository();
            WeatherData = currentWeather.First();
            WeatherDailyData = ContainsDailyForecasts
                ? await _repositoryStore.DailyForecastsRepository.GetWeatherDataFromRepository() : null;
        }

        private bool IsWeatherDataCurrent()
        {
            return WeatherData != null && DateProvider.GetActualDateTime() - WeatherData.Date <= TimeSpan.FromHours(1);
        }

        private async Task CreateChart()
        {
            if (ContainsDailyForecasts)
                Chart = await CreateLinearChart(CreateDailyWeatherDataTemperatureChartEntries());
        }

        private Task<LineChart> CreateLinearChart(IEnumerable<ChartEntry> entries)
        {
            var chart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Straight,
                ValueLabelOrientation = Orientation.Horizontal,
                LineAreaAlpha = 0,
                BackgroundColor = SKColor.Empty,
                LabelOrientation = Orientation.Horizontal,
                LabelTextSize = 25,
                LineSize = 10,
                PointMode = PointMode.Circle,
                PointSize = 18
            };
            return Task.FromResult(chart);
        }

        private IEnumerable<ChartEntry> CreateDailyWeatherDataTemperatureChartEntries()
        {
            var data = new List<ChartEntry>();
            foreach (var weatherData in WeatherDailyData)
                data.Add(WeatherDailyDataTemperatureToChartEntry(weatherData));
            return data;
        }

        private ChartEntry WeatherDailyDataTemperatureToChartEntry(WeatherData data)
        {
            var entry = new ChartEntry(data.TemperatureMax.Value)
            {
                Label = data.Date.ToShortDateString(),
                ValueLabel = data.TemperatureMax.ToString(),
                Color = GetProperSKColorAccordingToTemperature(data.TemperatureMax)
            };
            return entry;
        }

        private SKColor GetProperSKColorAccordingToTemperature(Temperature temperature)
        {
            var tempValue = temperature.Value;
            if(tempValue > 35)
                return SKColor.Parse("#ffee58");
            if(tempValue > 30)
                return SKColor.Parse("#f9a825");
            if(tempValue > 25)
                return SKColor.Parse("#ef6c00");
            if(tempValue > 5)
                return SKColor.Parse("#9e9e9e");
            if(tempValue > 0)
                return SKColor.Parse("#1a237e");
            if(tempValue > -5)
                return SKColor.Parse("#1e88e5");
            return SKColor.Parse("#4dd0e1");
        }
    }
}