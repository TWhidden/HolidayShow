using System;
using System.Windows;

namespace HolidayShowEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {


                var bootstrapper = new Bootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                base.Shutdown();
            }
        }
    }
}
