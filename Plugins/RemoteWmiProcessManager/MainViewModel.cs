using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using RemoteWmiProcessManager.Annotations;
using RemoteWmiProcessManager.Models;
using RemoteWmiProcessManager.Services;

namespace RemoteWmiProcessManager
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableDictionary<uint, ProcessInfo> ProcessInfos { get; set; }
        public ICommand KillProcessCommand { get; set; }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (value == _errorMessage) return;
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        private readonly AutoResetEvent _rescanForcedEvent = new AutoResetEvent(true);
        private readonly RemoteWmiTaskManager _taskManager;

        public MainViewModel(IPAddress ip)
        {
            KillProcessCommand = new ProcessCommand(KillProcess);

            ProcessInfos = new ObservableDictionary<uint, ProcessInfo>();

            _taskManager = new RemoteWmiTaskManager(ip);

            new Thread(ScanProc) { IsBackground = true }.Start();
        }

        private void KillProcess(KeyValuePair<uint, ProcessInfo> node)
        {
            if (node.Equals(default(KeyValuePair<uint, ProcessInfo>))) return;

            _taskManager.KillProcess(node.Value.Name);
            _rescanForcedEvent.Set();
        }

        private void ScanProc()
        {
            while (true)
            {
                try
                {
                    _rescanForcedEvent.WaitOne(5000); //return value not used by design, just sleeping while someone forces rescan

                    var source = _taskManager.GetProcesses();

                    if (source.Count == 0) return;

                    foreach (var obj in source)
                    {
                        if (ProcessInfos.ContainsKey(obj.Key))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ReflectObjectInModel(obj.Value.Item1, ProcessInfos[obj.Key], obj.Value.Item2);
                            });
                        }
                        else
                        {
                            var pInfo = new ProcessInfo();
                            ReflectObjectInModel(obj.Value.Item1, pInfo, obj.Value.Item2);

                            Application.Current.Dispatcher.Invoke(() =>
                                ProcessInfos.Add(pInfo.Id, pInfo));
                        }
                    }

                    var pInfoList = ProcessInfos.ToList();

                    foreach (var keyValuePair in pInfoList)
                    {
                        if (!source.ContainsKey(keyValuePair.Key))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                                ProcessInfos.Remove(keyValuePair.Key));
                        }
                    }

                    ErrorMessage = string.Empty;
                }
                catch (Exception e)
                {
                    ErrorMessage = e.Message;
                }
            }
        }

        private static void ReflectObjectInModel(System.Management.ManagementObject obj, ProcessInfo pInfo, string owner)
        {
            pInfo.Id = (uint)obj["ProcessId"];
            pInfo.Name = (string)obj["Name"];
            pInfo.Owner = owner;
            pInfo.ExecPath = (string)obj["ExecutablePath"];
            pInfo.Memory = (ulong)obj["WorkingSetSize"] / 1024.0 / 1024.0;
            pInfo.ThreadCount = (uint)obj["ThreadCount"];
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ProcessCommand : ICommand
    {
        private readonly Action<KeyValuePair<uint, ProcessInfo>> _action;

        public ProcessCommand(Action<KeyValuePair<uint, ProcessInfo>> action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action((KeyValuePair<uint, ProcessInfo>)parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
