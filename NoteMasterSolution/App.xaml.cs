using System.Configuration;
using System.Data;
using System.Windows;
using NoteMaster.ViewModels;

namespace NoteMaster
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            mainWindow.Show();
        }
    }

}
