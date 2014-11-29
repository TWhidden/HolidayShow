using HolidayShowEditor.ViewModels;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : ISettingsView
    {
        public SettingsView()
        {
            InitializeComponent();

            var vm = Bootstrapper.ShellContainer.Resolve<ISettingsViewModel>();
            SetViewModel(vm);
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }
    }
}
