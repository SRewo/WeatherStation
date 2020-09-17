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
        private INavigationService _navigationService;

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
            MenuItems = new ObservableCollection<MenuItem>(CreateMenuItems());
        }

        private MenuItem[] CreateMenuItems()
        {
            var collection = new List<MenuItem>();
            var i = 0;
            foreach(var r in _repositories)
            {
                var item = new MenuItem
                {
                    Id = i++,
                    TargetView = nameof(MainPage),
                    Title = r.RepositoryName,
                    Parameters = new NavigationParameters()
                };
                item.Parameters.Add("repository", r);
                collection.Add(item);
            }
            return collection.ToArray();
        }

        public async Task OpenViewAsync()
        {
            if (SelectedItem == null)
                return;

            await _navigationService.NavigateAsync($"/DetailPage/NavigationPage/{SelectedItem.TargetView}", SelectedItem.Parameters);
        }
    }
}
