using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WeatherStation.App.Views;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.App.ViewModels
{
    public class MasterPageViewModel : BindableBase
    {
        private readonly IWeatherRepositoryStore[] _repositories;
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

        public MasterPageViewModel(IEnumerable<IWeatherRepositoryStore> weatherRepositories, INavigationService service)
        {
            _repositories = weatherRepositories.ToArray();
            _navigationService = service;
            OpenViewCommand = new DelegateCommand(async () => await OpenViewAsync());
            MenuItems = new ObservableCollection<MenuItem>(CreateMenuItems().Result);
        }

        public async Task<MenuItem[]> CreateMenuItems()
        {
            var collection = new List<MenuItem>();
            collection.AddRange(await CreateRepositoryMenuItems(_repositories));
            var settingsMenuItem = await CreateSettingsMenuItem();
            collection.Add(settingsMenuItem);
            return collection.ToArray();
        }

        private async Task<IList<MenuItem>> CreateRepositoryMenuItems(IEnumerable<IWeatherRepositoryStore> repositories)
        {
            var menuItems = new List<MenuItem>();
            foreach(var r in repositories) menuItems.Add(await CreateMenuItemFromRepository(r));
            return menuItems;
        }

        private Task<MenuItem> CreateMenuItemFromRepository(IWeatherRepositoryStore repositoryStore)
        {
            var item = new MenuItem
            {
                TargetView = nameof(MainPageView),
                Title = repositoryStore.RepositoryName,
                Parameters = new NavigationParameters {{"repositoryStore", repositoryStore}}
            };
            return Task.FromResult(item);
        }

        private Task<MenuItem> CreateSettingsMenuItem()
        {
            var item = new MenuItem
            {
                TargetView = nameof(SettingsView),
                Title = "Settings",
            };
            return Task.FromResult(item);

        }

        public async Task OpenViewAsync()
        {
            if (SelectedItem != null)
                await _navigationService.NavigateAsync($"/DetailPageView/NavigationPage/{SelectedItem.TargetView}", SelectedItem.Parameters);
        }
    }
}
