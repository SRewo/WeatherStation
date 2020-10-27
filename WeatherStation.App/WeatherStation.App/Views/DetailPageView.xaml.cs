using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WeatherStation.App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPageView : MasterDetailPage , IMasterDetailPageOptions
    {
        public DetailPageView()
        {
            InitializeComponent();
        }

        public bool IsPresentedAfterNavigation => true;
    }
}