namespace HolidayShowEditor.Views
{
    /// <summary>
    /// Interaction logic for DeviceView.xaml
    /// </summary>
    public partial class DeviceView : IDeviceView
    {
        public DeviceView()
        {
            InitializeComponent();
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }
    }
}
