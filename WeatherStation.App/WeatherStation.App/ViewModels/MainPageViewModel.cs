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
using WeatherStation.App.Converters;
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
        private Chart _dailyTemperatureChart;
        private Chart _dailyRainChanceChart;


        private IEnumerable<WeatherData> _weatherDailyData;

        private WeatherData _weatherData;

        private IEnumerable<WeatherData> _weatherHourlyData;

        private Chart _chart;

        private bool _isTemperatureChartUsed;

        public MainPageViewModel(IDateProvider dateProvider, IPreferences preferences)
        {
            DateProvider = dateProvider;
            _preferences = preferences;
            GetDataCommand = new DelegateCommand(async () => { await GetData(); });
            ChangeChartCommand = new DelegateCommand(async () => { await ChangeChart(); });

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
        public DelegateCommand ChangeChartCommand { get; set; }

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

        public bool IsTemperatureChartUsed
        {
            get => _isTemperatureChartUsed;
            set => SetProperty(ref _isTemperatureChartUsed, value);
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

        private Task ChangeChart()
        {
            IsTemperatureChartUsed = !_isTemperatureChartUsed;
            Chart = IsTemperatureChartUsed ? _dailyTemperatureChart : _dailyRainChanceChart;

            return Task.CompletedTask;
        }

        private async Task CreateChart()
        {
            if (ContainsDailyForecasts)
                await CreateChartsForDailyForecasts();

            Chart = _dailyTemperatureChart;
        }

        private async Task CreateChartsForDailyForecasts()
        {
            _dailyTemperatureChart = await CreateDailyTemperatureLinearChart();

            _dailyRainChanceChart = await CreateDailyRainChanceBarChart();
        }

        private Task<LineChart> CreateDailyTemperatureLinearChart()
        {
            var converter = new DailyWeatherDataToTemperatureChartEntries();
            var temperatureChartEntries = converter.ConvertCollection(WeatherDailyData);
            return CreateLinearChart(temperatureChartEntries);
        }

        private Task<BarChart> CreateDailyRainChanceBarChart()
        {
            var converter = new DailyWeatherDataToRainChanceChartEntries();
            var chartEntries = converter.ConvertCollection(WeatherDailyData);
            return CreateBarChart(chartEntries);
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
            IsTemperatureChartUsed = true;
            return Task.FromResult(chart);
        }

        private Task<BarChart> CreateBarChart(IEnumerable<ChartEntry> chartEntries)
        {
            return Task.FromResult(new BarChart
            {
                Entries = chartEntries,
                MinValue = 0,
                MaxValue = 100
            });
        }
    }
}