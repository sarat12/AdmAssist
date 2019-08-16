using System.Windows;
using System.Windows.Markup;
using AdmAssist.ViewModels;
using AdmAssist.Views.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace AdmAssist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));
            
            base.OnStartup(e);
            //// get the current app style (theme and accent) from the application
            //// you can then use the current theme and custom accent instead set a new theme
            //Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);

            //// now set the Green accent and dark theme
            //ThemeManager.ChangeAppStyle(Application.Current,
            //                            ThemeManager.GetAccent("Green"),
            //                            ThemeManager.GetAppTheme("BaseDark")); // or appStyle.Item1
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainViewModel = new MainViewModel(DialogCoordinator.Instance);
            var mainWindow = new MainWindow(mainViewModel);
            mainWindow.Show();
        }
    }
}
