using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microcharts;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using WeatherStation.App.Utilities;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using Xamarin.Essentials.Interfaces;
using static WeatherStation.App.Weather;

namespace WeatherStation.App.ViewModels
{
    public class MainPageViewModel : BindableBase, INavigatedAware
    {
        private int _chartAndTitleHeight = 420;
        private readonly Task _prepareChartAndDataList;

        protected IDateProvider DateProvider;
        private IPreferences _preferences;
        private Chart _dailyTemperatureChart;

        private IEnumerable<WeatherMessage> _weatherDailyData;

        private WeatherMessage _weatherData;

        private IEnumerable<WeatherMessage> _weatherHourlyData;

        private Chart _chart;

        private bool _isTemperatureChartUsed;

        private bool _areHourlyForecastsSelected;

        private string _forecastsTitle;

        private int _dataListHeight;

        private bool _areBothForecastsTypesAvailable;

        private readonly WeatherClient _client;

        private Repositories _repositoryEnum;

        public MainPageViewModel(IDateProvider dateProvider, IPreferences preferences, IExceptionHandlingService service, WeatherClient weatherClient)
        {
            _client = weatherClient;
            DateProvider = dateProvider;
            _handlingService = service;
            _preferences = preferences;
            ChangeChartCommand = new DelegateCommand(async () => await ChangeChart());
            ChangeForecastsTypeCommand = new DelegateCommand(async () => await ChangeForecastsType());
            RefreshDataCommand = new DelegateCommand(async () => await RefreshData());

            CityName = _preferences.Get("CityName", "");
        }

        public WeatherMessage WeatherData
        {
            get => _weatherData;
            set => SetProperty(ref _weatherData, value);
        }

        public IEnumerable<WeatherMessage> WeatherHourlyData
        {
            get => _weatherHourlyData;
            set => SetProperty(ref _weatherHourlyData, value);
        }

        public bool ContainsDailyForecasts { get; set; }
        public bool ContainsHourlyForecasts { get; set; }
        public DelegateCommand ChangeChartCommand { get; set; }
        public DelegateCommand ChangeForecastsTypeCommand { get; set; }
        public DelegateCommand RefreshDataCommand { get; set; }


        public bool AreBothForecastTypesAvailable
        {
            get => _areBothForecastsTypesAvailable; 
            set => SetProperty(ref _areBothForecastsTypesAvailable, value);
        }

        public IEnumerable<WeatherMessage> WeatherDailyData
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
        private Chart _dailyRainChanceChart;
        private Chart _hourlyTemperatureChart;
        private Chart _hourlyRainChanceChart;
        private IExceptionHandlingService _handlingService;

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

        public string ForecastsTitle 
        {
            get => _forecastsTitle;
            set => SetProperty(ref _forecastsTitle, value);
        }

        public int DataListHeight
        {
            get => _dataListHeight;
            set => SetProperty(ref _dataListHeight, value);
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
            try
            {
                await PerformRequiredTasksOnViewLoad(parameters);
            }
            catch(Exception ex)
            {
                await _handlingService.HandleException(ex);
            }
        }

        private async Task PerformRequiredTasksOnViewLoad(INavigationParameters parameters)
        {
            await GetVariablesFromParameters(parameters);
            await CheckIfRepositoryContainsDailyAndHourlyForecasts();
            await GetData();

            await Task.WhenAll(
                    CreateChart(),
                    ChangeTitle(),
                    SetListAndChartViewHeight()
                    );
        }

        private Task GetVariablesFromParameters(INavigationParameters parameters)
        {
            _repositoryEnum = (Repositories) parameters["repository"];
            return Task.CompletedTask;
        }

        private async Task GetData()
        {
            await Task.WhenAll(
                    GetCurrentWeather(),
                    GetDailyForecasts(),
                    GetHourlyForecasts()
                );
        }

        private async Task GetCurrentWeather()
        {
            var request = new WeatherRequest{Repository = _repositoryEnum};
            var currentWeather = await _client.GetCurrentWeatherAsync(request);
            WeatherData = currentWeather.Weather;
        }

        private async Task GetDailyForecasts()
        {
            WeatherDailyData = ContainsDailyForecasts
                ? await GetDailyForecastsFromRepository()
                : null;
        }

        private async Task<IEnumerable<WeatherMessage>> GetDailyForecastsFromRepository()
        {
            var request = new WeatherRequest{Repository = _repositoryEnum};
            var result = await _client.GetDailyForecastsAsync(request);
            return result.Forecasts;
        }

        private async Task GetHourlyForecasts()
        {

            WeatherHourlyData = ContainsHourlyForecasts
                ? await GetHourlyForecastsFromRepository() 
                : null;
        }

        private async Task<IEnumerable<WeatherMessage>> GetHourlyForecastsFromRepository()
        {
            var request = new WeatherRequest{Repository = _repositoryEnum};
            var result = await _client.GetHourlyForecastsAsync(request);
            return result.Forecasts;
        }


        public async Task CheckIfRepositoryContainsDailyAndHourlyForecasts()
        {
            var request = new InfoRequest{Repository = _repositoryEnum};
            var repositoryInfo = await _client.GetRepositoryInfoAsync(request);

            ContainsDailyForecasts = repositoryInfo.ContainsDailyForecasts;
            ContainsHourlyForecasts = repositoryInfo.ContainsHourlyForecasts;

            AreBothForecastTypesAvailable = ContainsDailyForecasts && ContainsHourlyForecasts;
            AreHourlyForecastsSelected = !AreBothForecastTypesAvailable && ContainsHourlyForecasts;
        }


        private Task ChangeTitle()
        {
            ForecastsTitle = AreHourlyForecastsSelected ? "Hourly Forecasts" : "Daily Forecasts";

            return Task.CompletedTask;
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

        public async Task CreateChart()
        {
            var dailyChartsTask = ContainsDailyForecasts ? CreateChartsForDailyForecasts() : Task.CompletedTask;
            var hourlyChartsTask = ContainsHourlyForecasts ? CreateChartsForHourlyForecasts() : Task.CompletedTask;

            await Task.WhenAll(dailyChartsTask, hourlyChartsTask);

            Chart = ContainsDailyForecasts ? _dailyTemperatureChart : _hourlyRainChanceChart;
            IsTemperatureChartUsed = true;
        }

        private async Task CreateChartsForHourlyForecasts()
        {
            await Task.WhenAll(
                CreateRainChanceChartForHourlyForecasts(),
                CreateTemperatureChartForHourlyForecasts()
                );
        }

        private async Task CreateRainChanceChartForHourlyForecasts()
        {
            _hourlyRainChanceChart = await MainViewChartFactory.CreateRainChanceChart(
                new HourlyWeatherDataToRainChanceChartEntries(),
                WeatherHourlyData);
        }

        private async Task CreateTemperatureChartForHourlyForecasts()
        {
            _hourlyTemperatureChart = await MainViewChartFactory.CreateTemperatureChart(
                new HourlyWeatherDataToTemperatureChartEntries(),
                WeatherHourlyData);
        }

        private async Task CreateChartsForDailyForecasts()
        {
            await Task.WhenAll(
                CreateTemperatureChartForDailyForecasts(),
                CreateRainChanceChartForDailyForecasts()
                );
        }

        private async Task CreateRainChanceChartForDailyForecasts()
        {
            _dailyRainChanceChart = await MainViewChartFactory.CreateRainChanceChart(
                new DailyWeatherDataToRainChanceChartEntries(),
                WeatherDailyData);
        }

        private async Task CreateTemperatureChartForDailyForecasts()
        {
            _dailyTemperatureChart = await MainViewChartFactory.CreateTemperatureChart(
                new DailyWeatherDataToTemperatureChartEntries(),
                WeatherDailyData);
        }

        private async Task SetListAndChartViewHeight()
        {
            DataListHeight = _chartAndTitleHeight + await CalculateItemListHeight();
        }

        private Task<int> CalculateItemListHeight()
        {
            var numberOfItems = AreHourlyForecastsSelected ? WeatherHourlyData.Count() : WeatherDailyData.Count();
            var itemHeight = 67;

            return Task.FromResult(numberOfItems * itemHeight);
        }

        private async Task ChangeForecastsType()
        {
            if(!AreBothForecastTypesAvailable)
                return;

            AreHourlyForecastsSelected = !_areHourlyForecastsSelected;

            await Task.WhenAll(
                    CreateChart(),
                    ChangeTitle(),
                    SetListAndChartViewHeight()
                    );
        }

        private async Task RefreshData()
        {
            try
            {
                await RefreshViewData();
            }
            catch(Exception ex)
            {
                await _handlingService.HandleException(ex);
            }
        }

        private async Task RefreshViewData()
        {
                await ResetWeatherDataFields();
                await GetData();
                await CreateChart();
        }

        private Task ResetWeatherDataFields()
        {
            WeatherHourlyData = null;
            WeatherDailyData = null;
            WeatherData = null;
            return Task.CompletedTask;
        }
    }
}