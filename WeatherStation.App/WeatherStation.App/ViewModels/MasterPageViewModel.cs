using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WeatherStation.App.Views;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.App.ViewModels
{
    public class MasterPageViewModel : BindableBase
    {
        private readonly IWeatherRepository[] _repositories;
        private ObservableCollection<MenuItem> _menuItems;
        private MenuItem _selectedItem;
        private readonly INavigationService _navigationService;

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set => SetProperty(ref _menuItems, value);
        }

        public MenuItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public DelegateCommand OpenViewCommand { get; set; }

        public MasterPageViewModel(IWeatherRepository[] weatherRepositories, INavigationService service)
        {
            _repositories = weatherRepositories;
            _navigationService = service;
            OpenViewCommand = new DelegateCommand(async () => await OpenViewAsync());
            MenuItems = new ObservableCollection<MenuItem>(CreateMenuItems().Result);
        }

        private async Task<MenuItem[]> CreateMenuItems()
        {
            var collection = new List<MenuItem>();
            collection.AddRange(await CreateRepositoryMenuItems(_repositories));
            collection.AddRange(await CreateAdditionalMenuItems());
            return collection.ToArray();
        }

        public Task<MenuItem> CreateMenuItemFromRepository(IWeatherRepository repository)
        {
            var item = new MenuItem
            {
                TargetView = nameof(MainPageView),
                Title = repository.RepositoryName,
                Parameters = new NavigationParameters {{"repository", repository}}
            };
            return Task.FromResult(item);
        }

        public async Task<IList<MenuItem>> CreateRepositoryMenuItems(IEnumerable<IWeatherRepository> repositories)
        {
            var menuItems = new List<MenuItem>();
            foreach(var r in repositories) menuItems.Add(await CreateMenuItemFromRepository(r));
            return menuItems;
        }

        public async Task<IList<MenuItem>> CreateAdditionalMenuItems()
        {
            var additionalMenuItems = new List<MenuItem> {await CreateSettingsMenuItem()};
            return additionalMenuItems;
        }

        public Task<MenuItem> CreateSettingsMenuItem()
        {
            var settingsMenuItem = new MenuItem {TargetView = nameof(SettingsView), Title = "Settings"};
            return Task.FromResult(settingsMenuItem);
        }

        public async Task OpenViewAsync()
        {
            if (SelectedItem != null)
                await _navigationService.NavigateAsync($"/DetailPageView/NavigationPage/{SelectedItem.TargetView}", SelectedItem.Parameters);
        }
    }
}
