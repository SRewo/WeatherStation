using Prism;
using Prism.Ioc;
using WeatherStation.App.ViewModels;
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
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
            
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

           await  NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void OnResume()
        {
        }
    }
}
