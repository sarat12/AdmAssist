using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using AdmAssist.Services;
using AdmAssist.ViewModels;
using AdmAssist.Views.Converters;
using MahApps.Metro;
using RemoteQueries.Types;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;

namespace AdmAssist.Views.Pages
{
    public partial class SettingsPage
    {
        private MainViewModel MainViewModel => (MainViewModel)DataContext;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += Preferences_Loaded;
        }

        private void Preferences_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateScanningOptions();
            PopulateThemesStackPanel();
            PopulateAccentsStackPanel();
        }

        private void PopulateScanningOptions()
        {
            foreach (var queryParameter in MainViewModel.RemoteParameterSet)
            {
                StackPanel parent = null;

                switch (queryParameter.Group)
                {
                    case RemoteQueryGroups.Dns: parent = SpDns; break;
                    case RemoteQueryGroups.Arp: parent = SpArp; break;
                    case RemoteQueryGroups.NetBios: parent = SpNetBios; break;
                    case RemoteQueryGroups.RemoteRegistry: parent = SpRemoteRegistry; break;
                    case RemoteQueryGroups.Snmp: parent = SpSnmp; break;
                    case RemoteQueryGroups.Wmi: parent = SpWmi; break;
                }

                var cb = new CheckBox
                {
                    Content = queryParameter.Name,
                    FocusVisualStyle = null
                };
                cb.SetBinding(ToggleButton.IsCheckedProperty, new Binding
                {
                    Source = queryParameter,
                    Path = new PropertyPath("Enabled")
                });

                parent?.Children.Add(cb);
            }
        }

        private void PopulateThemesStackPanel()
        {
            foreach (var appTheme in ThemeManager.AppThemes)
            {
                var rb = new RadioButton
                {
                    Margin = new Thickness(2),
                    Content = appTheme.Name,
                    GroupName = "themes",
                    FocusVisualStyle = null
                };
                rb.Checked += ThemeRb_Checked;

                var binding = new Binding
                {
                    Path = new PropertyPath("Config.Theme"),
                    Converter = new StringToBooleanConverter(),
                    ConverterParameter = (string) rb.Content
                    
                };
                rb.SetBinding(ToggleButton.IsCheckedProperty, binding);

                SpThemes.Children.Add(rb);
            }
        }

        private void PopulateAccentsStackPanel()
        {
            foreach (var accent in ThemeManager.Accents)
            {
                var rb = new RadioButton
                {
                    Margin = new Thickness(2),
                    Content = accent.Name,
                    GroupName = "accents",
                    FocusVisualStyle = null,
                    Foreground = (Brush)accent.Resources["AccentBaseColorBrush"]
            };
                rb.Checked += AccentRb_Checked;

                var binding = new Binding
                {
                    Path = new PropertyPath("Config.Accent"),
                    Converter = new StringToBooleanConverter(),
                    ConverterParameter = (string)rb.Content

                };
                rb.SetBinding(ToggleButton.IsCheckedProperty, binding);

                SpAccents.Children.Add(rb);
            }
        }

        private void AccentRb_Checked(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent((string)rb.Content), ThemeManager.GetAppTheme(ConfigurationManager.Configuration.Theme));
        }

        private void ThemeRb_Checked(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton) sender;

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(ConfigurationManager.Configuration.Accent), ThemeManager.GetAppTheme((string)rb.Content));
        }

        private void TbSc_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // The text box grabs all input.
            e.Handled = true;

            // Fetch the actual shortcut key.
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            //Ignore modifier keys.
            if (Keyboard.Modifiers == 0 
                || key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            // Build the shortcut key name.
            var shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                shortcutText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                shortcutText.Append("Alt+");
            }
            shortcutText.Append(key);

            // Update the text box.
            TbSc.Text = shortcutText.ToString();
        }

        private void OpenExecutableClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {Filter = @"Executable Files|*.exe;*.msi;*.bat", Multiselect = false};

            if (dlg.ShowDialog() != DialogResult.OK) return;

            TbExe.Text = dlg.FileName;
        }
    }
}
