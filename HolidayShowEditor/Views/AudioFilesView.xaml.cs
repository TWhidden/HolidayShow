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
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }
    }
}
