using HolidayShowEditor.ViewModels;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor.Views
{
    /// <summary>
    /// Interaction logic for AudioFilesView.xaml
    /// </summary>
    public partial class AudioFilesView : IAudioFilesView
    {
        public AudioFilesView()
        {
            InitializeComponent();

            var vm = Bootstrapper.ShellContainer.Resolve<IAudioFilesViewModel>();
            SetViewModel(vm);
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }
    }
}
