using HolidayShowEditor.ViewModels;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor.Views
{
    /// <summary>
    /// Interaction logic for SetsView.xaml
    /// </summary>
    public partial class SetsView : ISetsView
    {
        public SetsView()
        {
            InitializeComponent();

            var vm = Bootstrapper.ShellContainer.Resolve<ISetsViewModel>();
            SetViewModel(vm);
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }
    }
}
