using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WeatherStation.App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPage : MasterDetailPage , IMasterDetailPageOptions
    {
        public DetailPage()
        {
            InitializeComponent();
        }

        public bool IsPresentedAfterNavigation => true;
    }
}