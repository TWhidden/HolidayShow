

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
        }

        public void SetViewModel(object viewModel)
        {
            this.DataContext = viewModel;
        }
    }
}
