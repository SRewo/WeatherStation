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
        private Chart _hourlyTemperatureChart;
        private Chart _hourlyRainChanceChart;


        private IEnumerable<WeatherData> _weatherDailyData;

        private WeatherData _weatherData;

        private IEnumerable<WeatherData> _weatherHourlyData;

        private Chart _chart;

        private bool _isTemperatureChartUsed;

        private bool _areHourlyForecastsSelected;

        public MainPageViewModel(IDateProvider dateProvider, IPreferences preferences)
        {
            DateProvider = dateProvider;
            _preferences = preferences;
            GetDataCommand = new DelegateCommand(async () => { await GetData(); });
            ChangeChartCommand = new DelegateCommand(async () => { await ChangeChart(); });
            ChangeForecastsTypeCommand = new DelegateCommand(async () => { await ChangeForecastsType(); });

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
        public DelegateCommand ChangeForecastsTypeCommand { get; set; }
        public bool AreBothForecastTypesAvailable { get; set; }

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

        public bool AreHourlyForecastsSelected
        {
            get => _areHourlyForecastsSelected;
            set => SetProperty(ref _areHourlyForecastsSelected, value);
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
            if (ContainsDailyForecasts && ContainsHourlyForecasts)
                AreBothForecastTypesAvailable = true;
            return Task.CompletedTask;
        }

        private async Task DownloadWeatherDataFromRepository()
        {
            var currentWeather = await _repositoryStore.CurrentWeatherRepository.GetWeatherDataFromRepository();
            WeatherData = currentWeather.First();
            WeatherDailyData = ContainsDailyForecasts
                ? await _repositoryStore.DailyForecastsRepository.GetWeatherDataFromRepository()
                : null;
            WeatherHourlyData = ContainsHourlyForecasts
                ? await _repositoryStore.HourlyForecastsRepository.GetWeatherDataFromRepository()
                : null;
        }

        private bool IsWeatherDataCurrent()
        {
            return WeatherData != null && DateProvider.GetActualDateTime() - WeatherData.Date <= TimeSpan.FromHours(1);
        }

        private async Task ChangeForecastsType()
        {
            AreHourlyForecastsSelected = !_areHourlyForecastsSelected;
            await ChangeChart();
        }

        private Task ChangeChart()
        {
            IsTemperatureChartUsed = !_isTemperatureChartUsed;
            if(!AreHourlyForecastsSelected)
                Chart = IsTemperatureChartUsed ? _dailyTemperatureChart : _dailyRainChanceChart;
            else
                Chart = IsTemperatureChartUsed ? _hourlyTemperatureChart : _hourlyRainChanceChart;

            return Task.CompletedTask;
        }

        private async Task CreateChart()
        {
            if (ContainsDailyForecasts)
                await CreateChartsForDailyForecasts();

            if (ContainsHourlyForecasts)
                await CreateChartsForHourlyForecasts();

            Chart = ContainsDailyForecasts ? _dailyTemperatureChart : _hourlyRainChanceChart;
        }

        private async Task CreateChartsForDailyForecasts()
        {
            var temperatureConverter = new DailyWeatherDataToTemperatureChartEntries();
            _dailyTemperatureChart = await CreateTemperatureLinearChart(temperatureConverter, WeatherDailyData);

            var rainChanceConverter = new DailyWeatherDataToRainChanceChartEntries();
            _dailyRainChanceChart = await CreateRainChanceBarChart(rainChanceConverter, WeatherDailyData);
        }

        private Task<LineChart> CreateTemperatureLinearChart(WeatherDataToChartEntryConverter converter, IEnumerable<WeatherData> data)
        {
            var temperatureChartEntries = converter.ConvertCollection(data);
            return CreateLinearChart(temperatureChartEntries);
        }

        private Task<BarChart> CreateRainChanceBarChart(WeatherDataToChartEntryConverter converter, IEnumerable<WeatherData> data)
        {
            var chartEntries = converter.ConvertCollection(data);
            return CreateBarChart(chartEntries);
        }

        private async Task CreateChartsForHourlyForecasts()
        {
            var temperatureConverter = new HourlyWeatherDataToTemperatureChartEntries();
            _hourlyTemperatureChart = await CreateTemperatureLinearChart(temperatureConverter, WeatherHourlyData);

            var rainChanceConverter = new HourlyWeatherDataToRainChanceChartEntries();
            _hourlyRainChanceChart = await CreateRainChanceBarChart(rainChanceConverter, WeatherHourlyData);
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