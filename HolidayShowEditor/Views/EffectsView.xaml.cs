using HolidayShowEditor.ViewModels;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor.Views
{
    /// <summary>
    /// Interaction logic for EffectsView.xaml
    /// </summary>
    public partial class EffectsView : IEffectsView
    {
        public EffectsView()
        {
            InitializeComponent();
            var vm = Bootstrapper.ShellContainer.Resolve<IEffectsViewModel>();
            SetViewModel(vm);
        }

        public void SetViewModel(object viewModel)
        {
            DataContext = viewModel;
        }
    }
}
