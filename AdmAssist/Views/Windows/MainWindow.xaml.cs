using AdmAssist.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System.ComponentModel;
using System.Windows;
using AdmAssist.Services;
using MahApps.Metro;

namespace AdmAssist.Views.Windows
{
    public partial class MainWindow
    {
        private MainViewModel MainViewModel => (MainViewModel)DataContext;

        public MainWindow(MainViewModel mainViewModel)
        {
            DataContext = mainViewModel;
            InitializeComponent();
            //DataContext = new MainViewModel(DialogCoordinator.Instance);
            ConfigurationManager.ConfigurationLoaded += Config_Loaded;
            ConfigurationManager.ConfigurationSaved += Config_Saved;
        }

        private void Config_Saved()
        {
        }

        private void Config_Loaded()
        {
            UpdateTheme();
        }

        private void UpdateTheme()
        {
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ConfigurationManager.Configuration.Accent), ThemeManager.GetAppTheme(ConfigurationManager.Configuration.Theme));
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true; ((MainViewModel)DataContext).CloseApp();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateTheme();
        }
    }
}
