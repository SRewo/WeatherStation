using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using RestSharp;
using Unity;
using WeatherStation.App.ViewModels;
using WeatherStation.App.Views;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using Xamarin.Forms;

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
            var accuRestClient = new RestClient("http://dataservice.accuweather.com");
            var dateProvider = new DateProvider();
            containerRegistry.GetContainer().AddNewExtension<Diagnostic>();
            containerRegistry.Register<IDateProvider, DateProvider>();
            containerRegistry.RegisterSingleton(typeof(IWeatherRepository),() => AccuWeatherRepository.CreateInstanceWithCityCode(AppApiKeys.AccuWeatherApiKey, "1411530", dateProvider, accuRestClient));

            containerRegistry.RegisterForNavigation<MainPageView, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<DetailPageView>();
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            var result = await NavigationService.NavigateAsync("DetailPage/NavigationPage/MainPage");
            if (result.Success) return;
            Console.WriteLine(result.Exception);
            Debugger.Break();
        }

        protected override void OnResume()
        {
        }
    }
}
