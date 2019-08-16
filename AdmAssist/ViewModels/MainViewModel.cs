using AdmAssist.Annotations;
using AdmAssist.Enums;
using AdmAssist.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdmAssist.Interfaces;
using AdmAssist.Services;
using RemoteQueries;

namespace AdmAssist.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private bool _stopScanningRequested;
        private string _acknowledgedFilterString;
        private readonly CustomDialog _settingsDialog;
        private ObservableCollection<IMenuTreeNode> _hostOperationsMenuTree;

        public Configuration Config => ConfigurationManager.Configuration;

        public ObservableCollection<NotifyDynamicDictionary> GridSource { get; }
        public AppStats AppStats { get; }

        public string AcknowledgedFilterString
        {
            get => _acknowledgedFilterString;
            set
            {
                if (value == _acknowledgedFilterString) return;
                _acknowledgedFilterString = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IpRangeViewModel> IpRanges { get; set; }
        public ObservableCollection<SelectableObject<string>> FilterColumns { get; set; }
        public ObservableCollection<RemoteQueryParameter> RemoteParameterSet { get; set; } // representation of all avaliable scanning parameters
        public Dictionary<string, Type> RemoteParametersTypes { get; set; } // dictionary of all scanning parameters types

        public ObservableCollection<IMenuTreeNode> HostOperationsMenuTree
        {
            get
            { return _hostOperationsMenuTree ?? (_hostOperationsMenuTree = new ObservableCollection<IMenuTreeNode>()); }
            set
            {
                if (Equals(value, _hostOperationsMenuTree)) return;
                _hostOperationsMenuTree = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(IDialogCoordinator instance)
        {
            //HostOperationsMenuTree = new ObservableCollection<IMenuTreeNode>();
            LoadAvaliableParametersAndTypes();

            ConfigurationManager.ConfigurationLoaded += Config_Loaded;
            ConfigurationManager.ConfigurationSaved += Config_Saved;
            ConfigurationManager.LoadConfiguration(true);

            _settingsDialog = new CustomDialog();

            GridSource = new ObservableCollection<NotifyDynamicDictionary>();
            FilterColumns = new ObservableCollection<SelectableObject<string>>();
            _dialogCoordinator = instance;
            AppStats = new AppStats();

            InitCommands();

            LoadIpAddressRangesFromConfig();
        }

        private void Config_Saved()
        {
            InitHostCommands(HostOperationsMenuTree);
        }

        private void Config_Loaded()
        {
            HostOperationsMenuTree = Config.MenuTree;
            InitHostCommands(HostOperationsMenuTree);
            ReflectRemoteParameterSetAccordingToUserSet();
        }

        public async void CloseApp()
        {
            await Task.Run(() =>
            {
                ConfigurationManager.StoreConfiguration(false);

                if (AppStats.ApplicationState == AppState.Chilling)
                    Environment.Exit(0);

                RequestStop();
                ForceIfNeeded();

                _currentTaskIsFinishedEvent.WaitOne();
                Environment.Exit(0);
            });
        }

        private async void ForceIfNeeded()
        {
            var res = await _dialogCoordinator.ShowMessageAsync(this, "Closing..", "It's better to wait for all threads are finished with their tasks!",
                MessageDialogStyle.Affirmative, new MetroDialogSettings { AffirmativeButtonText = "Force" });

            if (res == MessageDialogResult.Affirmative) Environment.Exit(0);
        }

        #region Private Methods

        private void LoadIpAddressRangesFromConfig()
        {
            IpRanges = new ObservableCollection<IpRangeViewModel>();
            foreach (var ipAddressRange in Config.IpAddressRanges)
            {
                IpRanges.Add(new IpRangeViewModel { Left = ipAddressRange.Begin.ToString(), Right = ipAddressRange.End.ToString() });
            }
        }

        private void LoadAvaliableParametersAndTypes()
        {
            RemoteParameterSet = new ObservableCollection<RemoteQueryParameter>();
            RemoteParametersTypes = new Dictionary<string, Type>();

            foreach (var remoteParameter in ParameterSet.Set)
            {
                RemoteParameterSet.Add(new RemoteQueryParameter(remoteParameter.Name, remoteParameter.Group));
                RemoteParametersTypes.Add(remoteParameter.Name, remoteParameter.Type);
            }
        }

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
