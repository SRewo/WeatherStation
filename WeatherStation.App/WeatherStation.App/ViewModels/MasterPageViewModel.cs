using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WeatherStation.App.Utilities;
using WeatherStation.App.Views;
using static WeatherStation.App.Weather;

namespace WeatherStation.App.ViewModels
{
    public class MasterPageViewModel : BindableBase
    {
        private ObservableCollection<MenuItem> _menuItems;
        private MenuItem _selectedItem;
        private readonly INavigationService _navigationService;
        private readonly IExceptionHandlingService _exceptionHandlingService;

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

        public MasterPageViewModel(INavigationService service, IExceptionHandlingService exceptionHandling)
        {
            _exceptionHandlingService = exceptionHandling;
            _navigationService = service;
            OpenViewCommand = new DelegateCommand(async () => await OpenViewAsync());
            MenuItems = new ObservableCollection<MenuItem>(CreateMenuItems().Result);
        }
        
        public async Task<MenuItem[]> CreateMenuItems()
        {
            try
            {
                return await CreateMenuItemsArray();
            }
            catch(Exception ex)
            {
                await _exceptionHandlingService.HandleException(ex);
            }
            return new MenuItem[0];
        }

        private async Task<MenuItem[]> CreateMenuItemsArray()
        {
            var collection = new List<MenuItem>();
            collection.AddRange(await CreateRepositoryMenuItems());
            return collection.ToArray();
        }

        private async Task<IList<MenuItem>> CreateRepositoryMenuItems()
        {
            var menuItems = new List<MenuItem>();
            var repositories = Enum.GetValues(typeof(Repositories)).Cast<Repositories>();
            foreach(var r in repositories) menuItems.Add(await CreateMenuItemFromRepository(r));
            return menuItems;
        }

        private Task<MenuItem> CreateMenuItemFromRepository(Repositories repositoryEnum)
        {
            var item = new MenuItem
            {
                TargetView = nameof(MainPageView),
                Title = repositoryEnum.ToString(),
                Parameters = new NavigationParameters {{"repository", repositoryEnum}}
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
