using AdmAssist.Enums;
using AdmAssist.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdmAssist.Helpers;
using AdmAssist.Interfaces;
using AdmAssist.Services;
using AdmAssist.Views.Pages;

namespace AdmAssist.ViewModels
{
    public delegate void ScanningPrcessStartedEventHandler();

    public partial class MainViewModel
    {
        public ICommand ScanCommand { get; private set; }
        public ICommand AddIpRangeCommand { get; private set; }
        public ICommand RemoveIpRangeCommand { get; private set; }
        public ICommand StopScanningCommand { get; private set; }
        public ICommand ShowSettingsDialogCommand { get; private set; }
        public ICommand CancelSettingsCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand AddNewHostCommandCommand { get; private set; }
        public ICommand AddNewMenuTreeNodeCommand { get; private set; }
        public ICommand DeleteIMenuTreeNodeCommand { get; private set; }
        public ICommand MoveMenuTreeNodeUpCommand { get; private set; }
        public ICommand MoveMenuTreeNodeDownCommand { get; private set; }

        private static readonly object ThreadCounterSyncObj = new object();
        private readonly AutoResetEvent _currentTaskIsFinishedEvent = new AutoResetEvent(true);
        private static readonly ConcurrentQueue<ScanTask> ScanTasks = new ConcurrentQueue<ScanTask>();
        private static int _scannedNodesCount;

        public event ScanningPrcessStartedEventHandler ScanningProcessStarted;

        #region Command Methods

        private void MoveMenuTreeNodeDown(object obj)
        {
            var treeView = (TreeView)obj;
            var selectedTreeItem = treeView.SelectedItem as IMenuTreeNode;

            if (selectedTreeItem == null) return;

            var hostCommandNodeCollection = FindHostCommandNodeCollection(selectedTreeItem, HostOperationsMenuTree);

            var currentId = hostCommandNodeCollection.IndexOf(selectedTreeItem);

            if (currentId >= hostCommandNodeCollection.Count - 1) return;

            hostCommandNodeCollection.RemoveAt(currentId);
            hostCommandNodeCollection.Insert(++currentId, selectedTreeItem);

            treeView.SetSelectedItem(selectedTreeItem);
        }

        private void MoveMenuTreeNodeUp(object obj)
        {
            var treeView = (TreeView)obj;
            var selectedTreeItem = treeView.SelectedItem as IMenuTreeNode;

            if (selectedTreeItem == null) return;

            var hostCommandNodeCollection = FindHostCommandNodeCollection(selectedTreeItem, HostOperationsMenuTree);

            var currentId = hostCommandNodeCollection.IndexOf(selectedTreeItem);

            if (currentId < 1) return;

            hostCommandNodeCollection.RemoveAt(currentId);
            hostCommandNodeCollection.Insert(--currentId, selectedTreeItem);

            treeView.SetSelectedItem(selectedTreeItem);
        }

        private void DeleteIMenuTreeNode(object obj)
        {
            var treeView = (TreeView)obj;
            var selectedTreeItem = treeView.SelectedItem as IMenuTreeNode;

            if (selectedTreeItem == null) return;

            var hostCommandNodeCollection = FindHostCommandNodeCollection(selectedTreeItem, HostOperationsMenuTree);
            hostCommandNodeCollection.Remove(selectedTreeItem);
        }

        private void AddNewMenuTreeNode(object obj)
        {
            var treeView = (TreeView)obj;
            var selectedTreeItem = treeView.SelectedItem as IMenuTreeNode;

            var newMenuTreeNode = new MenuTreeNode();

            if (selectedTreeItem == null)
            {
                HostOperationsMenuTree.Add(newMenuTreeNode);
            }
            else if (selectedTreeItem is MenuTreeNode)
            {
                (selectedTreeItem as MenuTreeNode).Children.Add(newMenuTreeNode);
            }
            else if (selectedTreeItem is HostCommandNode)
            {
                var hostCommandNodeCollection = FindHostCommandNodeCollection(selectedTreeItem, HostOperationsMenuTree);
                hostCommandNodeCollection.Add(newMenuTreeNode);
            }
            treeView.SetSelectedItem(newMenuTreeNode);
        }

        private void AddNewHostCommand(object obj)
        {
            var treeView = (TreeView)obj;
            var selectedTreeItem = treeView.SelectedItem as IMenuTreeNode;

            var newHostNode = new HostCommandNode();

            if (selectedTreeItem == null)
            {
                HostOperationsMenuTree.Add(newHostNode);
            }
            else if (selectedTreeItem is MenuTreeNode)
            {
                (selectedTreeItem as MenuTreeNode).Children.Add(newHostNode);
            }
            else if (selectedTreeItem is HostCommandNode)
            {
                var hostCommandNodeCollection = FindHostCommandNodeCollection(selectedTreeItem as HostCommandNode, HostOperationsMenuTree);
                hostCommandNodeCollection.Add(newHostNode);
            }
            treeView.SetSelectedItem(newHostNode);
        }

        private void RequestStop()
        {
            if (AppStats.ApplicationState == AppState.Chilling)
                return;

            AppStats.ApplicationState = AppState.Stopping;
            _stopScanningRequested = true;
        }

        private async void Scan()
        {
            FilterColumns.Clear();

            if (!ValidateIpRanges())
                return;

            if (AppStats.ApplicationState == AppState.Supervising)
                RequestStop();

            await Task.Run(() =>
            {
                while (AppStats.CurrentlyRunningThreadsCount > 0) // to avoid concurency when manual rescan invoked TODO: develop some better solution
                    Thread.Sleep(100);

                if (_currentTaskIsFinishedEvent.WaitOne())
                {
                    if (AppStats.ApplicationState != AppState.Chilling &&
                        AppStats.ApplicationState != AppState.Supervising)
                    {
                        _currentTaskIsFinishedEvent.Set();
                        return;
                    }

                    ScanningProcessStarted?.Invoke();

                    ResetScannedNodesCount();

                    AppStats.ApplicationState = AppState.Scaning;

                    Application.Current.Dispatcher.Invoke(() => GridSource.Clear());
                    
                    GenerateRowsFromIpRangesAndArangeScanTasks();

                    CreateFileterColumnsList();

                    var requiredThreadsCount = (byte)Math.Min(GridSource.Count, Config.MaxThreads);

                    var multiTask = new MultiThreadedTask(requiredThreadsCount);
                    multiTask.Finished += MultiTask_Finished;

                    multiTask.Begin(ScanProc);
                }
            });
        }

        private void SaveSettings()
        {
            ReflectUserSetAccordingToRemoteParameterSet();
            ConfigurationManager.StoreConfiguration(true);
            _dialogCoordinator.HideMetroDialogAsync(this, _settingsDialog);
        }

        private void CancelSettings()
        {
            ConfigurationManager.LoadConfiguration(true);
            _dialogCoordinator.HideMetroDialogAsync(this, _settingsDialog);
        }

        private async void ShowSettingsDialog()
        {
            _settingsDialog.DialogTop = new SettingsPage { DataContext = this };
            await _dialogCoordinator.ShowMetroDialogAsync(this, _settingsDialog);
        }

        private void RemoveIpRange(object obj)
        {
            if (IpRanges.Count <= 1)
            {
                _dialogCoordinator.ShowMessageAsync(this, "Oops..", "You can't remove the last ip range.");
                return;
            }

            var ipRange = (IpRangeViewModel)obj;

            IpRanges.Remove(ipRange);
        }

        private void AddIpRange()
        {
            const byte maxIpRanges = byte.MaxValue;

            if (IpRanges.Count < maxIpRanges)
                IpRanges.Add(new IpRangeViewModel());
        }

        #endregion

        #region Private Methods

        private static ObservableCollection<IMenuTreeNode> FindHostCommandNodeCollection(IMenuTreeNode menuNode, ObservableCollection<IMenuTreeNode> collection)
        {
            foreach (IMenuTreeNode menuTreeNode in collection)
            {
                if (menuTreeNode is MenuTreeNode)
                {
                    var res = FindHostCommandNodeCollection(menuNode, (menuTreeNode as MenuTreeNode).Children);
                    if (res != null) return res;
                }
                if (menuTreeNode == menuNode)
                    return collection;
            }
            return null;
        }

        private void ResetScannedNodesCount()
        {
            _scannedNodesCount = 0;
        }

        private static void AddTask(NotifyDynamicDictionary node)
        {
            ScanTasks.Enqueue(new ScanTask(node));
        }

        private void InitCommands()
        {
            ScanCommand = new RelayCommand(Scan);
            AddIpRangeCommand = new RelayCommand(AddIpRange);
            RemoveIpRangeCommand = new ParametrizedRelayCommand(RemoveIpRange);
            StopScanningCommand = new RelayCommand(RequestStop);
            ShowSettingsDialogCommand = new RelayCommand(ShowSettingsDialog);
            CancelSettingsCommand = new RelayCommand(CancelSettings);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            AddNewHostCommandCommand = new ParametrizedRelayCommand(AddNewHostCommand);
            AddNewMenuTreeNodeCommand = new ParametrizedRelayCommand(AddNewMenuTreeNode);
            DeleteIMenuTreeNodeCommand = new ParametrizedRelayCommand(DeleteIMenuTreeNode);
            MoveMenuTreeNodeUpCommand = new ParametrizedRelayCommand(MoveMenuTreeNodeUp);
            MoveMenuTreeNodeDownCommand = new ParametrizedRelayCommand(MoveMenuTreeNodeDown);
        }

        private void ScanProc()
        {
            IncreseScanningThreadsCount();

            while (ScanTasks.Count != 0)
            {
                if (_stopScanningRequested) break;

                ScanTasks.TryDequeue(out ScanTask task);

                NodesServer.ScanNode(task);

                RecalculateProgress();
            }

            DecreseScanningThreadsCount();
        }

        private void SuperviseProc()
        {
            IncreseScanningThreadsCount();

            foreach (var row in GridSource)
            {
                if (_stopScanningRequested) break;

                if (Monitor.TryEnter(row, 0))
                {
                    NodesServer.ScanNode(new ScanTask(row));

                    Monitor.Exit(row);
                }
            }

            DecreseScanningThreadsCount();
        }

        private void RescanProc(IEnumerable<NotifyDynamicDictionary> selectedHosts)
        {
            IncreseScanningThreadsCount();

            foreach (var row in selectedHosts)
            {
                if (_stopScanningRequested) break;

                if (Monitor.TryEnter(row, 0))
                {
                    NodesServer.ScanNode(new ScanTask(row));

                    Monitor.Exit(row);
                }
            }

            DecreseScanningThreadsCount();
        }

        private void RecalculateProgress()
        {
            _scannedNodesCount++;

            AppStats.CurrentScanningProgress =
                        (int)((double)_scannedNodesCount / GridSource.Count * 100);
        }

        private void MultiTask_Finished()
        {
            FinalizeScanningProcess();

            if (Config.ScanningOptions.AllowSupervising)
                StartSupervising();
        }

        private void FinalizeScanningProcess()
        {
            _currentTaskIsFinishedEvent.Set();
            AppStats.CurrentScanningProgress = 100;
            AppStats.ApplicationState = AppState.Chilling;
            _stopScanningRequested = false;
        }

        private void StartSupervising()
        {
            AppStats.ApplicationState = AppState.Supervising;
            new Thread(() =>
            {
                if (_currentTaskIsFinishedEvent.WaitOne())
                    while (!_stopScanningRequested)
                        SuperviseProc();

                AppStats.ApplicationState = AppState.Chilling;
                _stopScanningRequested = false;

                _currentTaskIsFinishedEvent.Set();

            })
            { IsBackground = true }.Start();
        }

        private void IncreseScanningThreadsCount()
        {
            lock (ThreadCounterSyncObj)
            {
                AppStats.CurrentlyRunningThreadsCount++;
            }
        }
        private void DecreseScanningThreadsCount()
        {
            lock (ThreadCounterSyncObj)
            {
                AppStats.CurrentlyRunningThreadsCount--;
            }
        }

        private void CreateFileterColumnsList()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilterColumns.Clear();
                FilterColumns.Add(new SelectableObject<string>("<all>"));

                foreach (var remoteParameter in Config.UserQuerySet)
                    if (RemoteParametersTypes[remoteParameter] == typeof(string))
                        FilterColumns.Add(new SelectableObject<string>(remoteParameter));
            });
        }

        private void GenerateRowsFromIpRangesAndArangeScanTasks()
        {
            foreach (var ipRange in Config.IpAddressRanges)
            {
                int start = ipRange.Begin.ToInt();
                int end = ipRange.End.ToInt();

                for (int i = start; i <= end; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(i);

                    var ip = new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] });

                    var newRow = GenerateNodeFromUserParameterSet(ip);

                    Application.Current.Dispatcher.Invoke(() =>
                        GridSource.Add(newRow));

                    AddTask(newRow);
                }
            }
        }

        private NotifyDynamicDictionary GenerateNodeFromUserParameterSet(IPAddress nodeIp)
        {
            var node = new NotifyDynamicDictionary
            {
                [Constants.IpColumnName] = nodeIp,
                [Constants.StatusColumnName] = null
            };

            foreach (var parameterName in Config.UserQuerySet)
                node[parameterName] = null;

            return node;
        }

        private bool ValidateIpRanges()
        {
            var rangesToSave = new List<IpAddressRange>();

            foreach (var ipRange in IpRanges)
            {
                IPAddress ip1;
                IPAddress ip2;

                if (!IPAddress.TryParse(ipRange.Left, out ip1) || !IPAddress.TryParse(ipRange.Right, out ip2))
                {
                    _dialogCoordinator.ShowMessageAsync(this, "Oops..", "Some of IP address ranges has not valid IP addresses");
                    return false;
                }

                int start = ip1.ToInt();
                int end = ip2.ToInt();

                if (end < start)
                {
                    _dialogCoordinator.ShowMessageAsync(this, "Oops..", "In some of IP address ranges end IP address grater than start");
                    return false;
                }

                rangesToSave.Add(new IpAddressRange(ip1, ip2));
            }

            Config.IpAddressRanges = rangesToSave;
            ConfigurationManager.StoreConfiguration(false);

            return true;
        }

        private void ReflectRemoteParameterSetAccordingToUserSet()
        {
            foreach (var remoteQueryParameter in RemoteParameterSet) // reset first
                remoteQueryParameter.Enabled = false;

            foreach (var paramName in Config.UserQuerySet)
            {
                RemoteParameterSet.First(p => p.Name == paramName).Enabled = true;
            }
        }

        private void ReflectUserSetAccordingToRemoteParameterSet()
        {
            Config.UserQuerySet.Clear(); // resets columns order

            foreach (var remoteQueryParameter in RemoteParameterSet)
            {
                if (remoteQueryParameter.Enabled)
                    Config.UserQuerySet.Add(remoteQueryParameter.Name);
            }
        }

        #endregion

    }

    #region ICommand Implementations

    public class RelayCommand : ICommand
    {
        private readonly Action _action;

        public RelayCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ParametrizedRelayCommand : ICommand
    {
        private readonly Action<object> _action;

        public ParametrizedRelayCommand(Action<object> action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    #endregion

}
