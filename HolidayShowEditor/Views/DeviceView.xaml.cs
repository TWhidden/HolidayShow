using HolidayShowEditor.ViewModels;
using Microsoft.Practices.Unity;

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
            var vm = Bootstrapper.ShellContainer.Resolve<IDeviceViewModel>();
            SetViewModel(vm);
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }
    }
}
