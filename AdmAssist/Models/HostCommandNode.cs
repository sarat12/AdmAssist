using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AdmAssist.Annotations;
using AdmAssist.Interfaces;
using Newtonsoft.Json;

namespace AdmAssist.Models
{
    public class HostCommandNode : IMenuTreeNode
    {
        private string _name;
        private bool _isDoubleClick;
        private string _shortcut;
        private bool _waitProcessExit;
        private ICommand _hostCommand;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsDoubleClick
        {
            get { return _isDoubleClick; }
            set
            {
                if (value == _isDoubleClick) return;
                _isDoubleClick = value;
                OnPropertyChanged();
            }
        }

        public string Shortcut
        {
            get { return _shortcut; }
            set
            {
                if (value == _shortcut) return;
                _shortcut = value;
                OnPropertyChanged();
            }
        }

        public bool HidePocessWindow { get; set; }

        public bool WaitProcessExit
        {
            get { return _waitProcessExit; }
            set
            {
                if (value == _waitProcessExit) return;
                _waitProcessExit = value;
                OnPropertyChanged();
            }
        }

        public bool RedirectOutputToLog { get; set; }

        public bool RequiresRescanAfterExecution { get; set; }

        public string Executable { get; set; }
        public string Argumets { get; set; }

        [JsonIgnore]
        public ICommand HostCommand
        {
            get { return _hostCommand; }
            set
            {
                if (Equals(value, _hostCommand)) return;
                _hostCommand = value;
                OnPropertyChanged();
            }
        }

        public HostCommandNode()
        {
            Name = "New Command";
        }

        #region INotifyPropertyChange Members

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
