using System.Globalization;
using System.Threading.Tasks;
using Foundation;
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
using WeatherStation.Library.Repositories.Weatherbit;
using Xamarin.Essentials;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using DeviceInfo = Xamarin.Essentials.DeviceInfo;

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

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDateProvider, DateProvider>();
            containerRegistry.Register<IAlertService, AlertService>();
            RegisterXamarinEssentialsTypes(containerRegistry);
            RegisterRepositories(containerRegistry);
            RegisterViewsForNavigation(containerRegistry);
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
            RegisterWeatherbitRepository(containerRegistry);
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

        private void RegisterWeatherbitRepository(IContainerRegistry containerRegistry)
        {
            var restClient = new RestClient("http://api.weatherbit.io/v2.0/");
            var coordinates = new Coordinates(Preferences.Get("lat",0.0), Preferences.Get("lon",0.0));
            var language = GetLanguageCode();
            containerRegistry.RegisterInstance<IWeatherRepositoryStore>(
                new WeatherbitRepositoryStore(restClient, 
                    AppApiKeys.WeatherbitApiKey, 
                    coordinates, Container.Resolve<IDateProvider>(),language), "Weatherbit");
        }

        private void RegisterGeocodingRepository(IContainerRegistry containerRegistry)
        {
            var geocodingRestClient = new RestClient("http://api.positionstack.com/v1/");
            containerRegistry.RegisterSingleton(typeof(IGeocodingRepository), 
                () => new PositionstackGeocodingRepository(geocodingRestClient, AppApiKeys.PositionstackApiKey));
        }

        private string GetLanguageCode()
        {
            return DeviceInfo.Platform.Equals(DevicePlatform.Android) ? CultureInfo.CurrentUICulture.IetfLanguageTag : NSLocale.PreferredLanguages[0];
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();
            await NavigateToMainView();
        }

        private Task<INavigationResult> NavigateToMainView()
        {
            var parameters = new NavigationParameters {{"repositoryStore", Container.Resolve<IWeatherRepositoryStore>("Weatherbit")}};
            return NavigationService.NavigateAsync("DetailPageView/NavigationPage/MainPageView", parameters);
        }

        protected override void OnResume()
        {
        }
    }
}
