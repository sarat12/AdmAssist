using System;
using System.Net;
using System.Windows;

namespace RemoteWmiProcessManager
{
    public partial class App
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            IPAddress ip = IPAddress.Any;

            if (e.Args.Length == 0 || !IPAddress.TryParse(e.Args[0], out ip)) Environment.Exit(0);

            var mainWindow = new MainWindow(new MainViewModel(ip)) {Title = $"Processes on: {ip}"};
            MainWindow = mainWindow;

            MainWindow.Show();
        }
    }
}
