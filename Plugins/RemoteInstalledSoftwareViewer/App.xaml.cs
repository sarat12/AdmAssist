using System;
using System.Net;
using System.Windows;

namespace RemoteInstalledSoftwareViewer
{
    public partial class App
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            IPAddress ip = IPAddress.Any;

            if (e.Args.Length == 0 || !IPAddress.TryParse(e.Args[0], out ip)) Environment.Exit(0);

            var mvm = new MainViewModel(ip);

            var mainWindow = new MainWindow { Title = $"Software on: {ip}", DataContext = mvm };
            MainWindow = mainWindow;

            MainWindow.Show();
        }
    }
}
