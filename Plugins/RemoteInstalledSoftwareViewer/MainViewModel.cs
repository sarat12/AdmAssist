using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RemoteInstalledSoftwareViewer.Annotations;
using RemoteInstalledSoftwareViewer.Models;
using Microsoft.Win32;

namespace RemoteInstalledSoftwareViewer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IPAddress _ip;
        public ObservableCollection<RemoteProgram> Programs { get; set; }
        public ICommand GetInstalledSoftwareListCommand { get; set; }

        public MainViewModel(IPAddress ip)
        {
            _ip = ip;
            Programs = new ObservableCollection<RemoteProgram>();
            GetInstalledSoftwareListCommand = new ParameterlessCommand(GetInstalledSoftwareList);

            GetInstalledSoftwareList();
        }

        private void GetInstalledSoftwareList()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            Programs.Clear();

            try
            {
                var regView = CheckArchitecture();

                GetInstalledSoftwareListByArch(RegistryView.Registry32);

                if (regView == RegistryView.Registry64)
                    GetInstalledSoftwareListByArch(RegistryView.Registry64);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


            Mouse.OverrideCursor = null;
        }

        private RegistryView CheckArchitecture()
        {
            using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, _ip.ToString()))
            using (var key = reg.OpenSubKey(@"System\CurrentControlSet\Control\Session Manager\Environment\"))
            {
                if (key != null)
                    return (string)key.GetValue("PROCESSOR_ARCHITECTURE") == "x86" ? RegistryView.Registry32 : RegistryView.Registry64;
            }
            return RegistryView.Default;
        }

        private void GetInstalledSoftwareListByArch(RegistryView rh)
        {
            DateTime? date = null;

            using (var reg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, _ip.ToString(), rh))
            using (var rk = reg.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (rk == null) return;
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        if (sk == null) continue;

                        var name = (string)sk.GetValue("DisplayName");

                        if (string.IsNullOrWhiteSpace(name)) continue;

                        var instLocation = (string)sk.GetValue("InstallLocation");
                        var uninstallStr = (string)sk.GetValue("UninstallString");
                        var vendor = (string)sk.GetValue("Publisher");
                        var version = (string)sk.GetValue("DisplayVersion");

                        var d = sk.GetValue("InstallDate");

                        if (d != null)
                        {
                            var dateStr = d as string;
                            if (dateStr != null)
                            {
                                if (!string.IsNullOrWhiteSpace(dateStr) && dateStr.Length == 8)
                                {
                                    string s = dateStr;
                                    int year = int.Parse(s.Substring(0, 4));
                                    int month = int.Parse(s.Substring(4, 2));
                                    int day = int.Parse(s.Substring(6, 2));
                                    date = new DateTime(year, month, day);
                                }
                            }
                            else
                            {
                                var startDate = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0), DateTimeKind.Utc);
                                var stringValue = d.ToString();
                                var regVal = Convert.ToInt64(stringValue);

                                var installDate = startDate.AddSeconds(regVal);

                                date = installDate.ToLocalTime();
                            }
                        }
                        Programs.Add(new RemoteProgram(uninstallStr, date, instLocation, name, vendor, version));
                    }
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ParameterlessCommand : ICommand
    {
        private readonly Action _action;

        public ParameterlessCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }
}
