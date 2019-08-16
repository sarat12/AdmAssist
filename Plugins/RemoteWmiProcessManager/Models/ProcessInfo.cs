using System.ComponentModel;
using System.Runtime.CompilerServices;
using RemoteWmiProcessManager.Annotations;

namespace RemoteWmiProcessManager.Models
{
    public class ProcessInfo : INotifyPropertyChanged
    {
        private uint _id;
        private string _name;
        private string _owner;
        private double _memory;
        private string _execPath;
        private uint _threadCount;

        public uint Id
        {
            get { return _id; }
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
            }
        }

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

        public string Owner
        {
            get { return _owner; }
            set
            {
                if (value == _owner) return;
                _owner = value;
                OnPropertyChanged();
            }
        }

        public double Memory
        {
            get { return _memory; }
            set
            {
                if (value.Equals(_memory)) return;
                _memory = value;
                OnPropertyChanged();
            }
        }

        public string ExecPath
        {
            get { return _execPath; }
            set
            {
                if (value == _execPath) return;
                _execPath = value;
                OnPropertyChanged();
            }
        }

        public uint ThreadCount
        {
            get { return _threadCount; }
            set
            {
                if (value == _threadCount) return;
                _threadCount = value;
                OnPropertyChanged();
            }
        }



        #region Prop Changed Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
