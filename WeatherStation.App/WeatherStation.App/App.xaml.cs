using System;
using System.Globalization;
using System.Threading.Tasks;
using Prism;
using Prism.Ioc;
using Prism.Navigation;
using RestSharp;
using WeatherStation.App.ViewModels;
using WeatherStation.App.Views;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using WeatherStation.Library.Repositories.AccuWeather;
using Xamarin.Essentials;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using DeviceInfo = Xamarin.Essentials.DeviceInfo;
using WeatherStation.App.Utilities;

namespace WeatherStation.App
{
    public partial class App
    {
        public App() : this(null)
        {
        }

        public App(IPlatformInitializer initializer) : base(initializer)
        {

        }

        protected override async void OnStart()
        {
            await Permissions.RequestAsync<Permissions.StorageWrite>();
            await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            await LogAppStarting();
        }

        private async Task LogAppStarting()
        {
            try
            {
                await App.Current.Container.Resolve<ILogger>().Log("App Started");
            }
            catch(Exception ex)
            {
                await App.Current.Container.Resolve<ExceptionHandlingService>().HandleException(ex);
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDateProvider, DateProvider>();
            containerRegistry.Register<IAlertService, AlertService>();
            RegisterExceptionHandlingServices(containerRegistry);
            RegisterXamarinEssentialsTypes(containerRegistry);
            RegisterRepositories(containerRegistry);
            RegisterViewsForNavigation(containerRegistry);
        }

        private void RegisterExceptionHandlingServices(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<System.IO.Abstractions.IFileSystem,System.IO.Abstractions.FileSystem>();

            containerRegistry.RegisterInstance<IWriteService>(new TxtWriteService(
                App.Current.Container.Resolve<System.IO.Abstractions.IFileSystem>(),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"Log-{DateTime.Now.ToString("g")}"));

            containerRegistry.Register<ILogger, Logger>();
            containerRegistry.Register<IErrorAlertService, ErrorAlertService>();
            containerRegistry.Register<IExceptionHandlingService, ExceptionHandlingService>();
        }

        private void RegisterViewsForNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainPageView, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<DetailPageView>();
            containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
        }

        private void RegisterRepositories(IContainerRegistry containerRegistry)
        {
            RegisterGeocodingRepository(containerRegistry);
            RegisterAccuWeatherRepository(containerRegistry);
        }

        private void RegisterXamarinEssentialsTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IPreferences, PreferencesImplementation>();
            containerRegistry.Register<IGeolocation, GeolocationImplementation>();
            containerRegistry.Register<IGeocoding, GeocodingImplementation>();
        }

        private void RegisterAccuWeatherRepository(IContainerRegistry containerRegistry)
        {
            var accuRestClient = new RestClient("http://dataservice.accuweather.com");
            var language = GetLanguageCode();
            containerRegistry.RegisterInstance<IWeatherRepositoryStore>(
                AccuWeatherRepositoryStore.FromCityCode(AppApiKeys.AccuWeatherApiKey,
                    Preferences.Get("AccuWeatherCityId", "1411530"),
                    Container.Resolve<IDateProvider>(),
                    accuRestClient, language).Result, "Accuweather");
        }

        private void RegisterGeocodingRepository(IContainerRegistry containerRegistry)
        {
            var geocodingRestClient = new RestClient("http://api.positionstack.com/v1/");
            containerRegistry.RegisterSingleton(typeof(IGeocodingRepository), 
                () => new PositionstackGeocodingRepository(geocodingRestClient, AppApiKeys.PositionstackApiKey));
        }

        private string GetLanguageCode()
        {
            return DeviceInfo.Platform.Equals(DevicePlatform.Android) ? CultureInfo.CurrentUICulture.IetfLanguageTag : "PL";
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();
            await NavigateToMainView();
        }

        private Task<INavigationResult> NavigateToMainView()
        {
            var parameters = new NavigationParameters {{"repositoryStore", Container.Resolve<IWeatherRepositoryStore>("Accuweather")}};
            return NavigationService.NavigateAsync("DetailPageView/NavigationPage/MainPageView", parameters);
        }

        protected override void OnResume()
        {
        }
    }
}
