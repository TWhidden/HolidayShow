using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HolidayShowEndpointUniversalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            // Not going to build up the infrastructure for just one view and view modol.
            // This will due for now.
            var viewModel = new MainPageViewModel {View = this};
            this.DataContext = viewModel;

        }
    }
}
