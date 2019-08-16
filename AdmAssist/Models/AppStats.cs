using AdmAssist.Annotations;
using AdmAssist.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AdmAssist.Models
{
    public class AppStats : INotifyPropertyChanged
    {
        private int _currentlyRunningThreadsCount;
        private int _currentScanningProgress;
        private AppState _applicationState;

        public int CurrentlyRunningThreadsCount
        {
            get { return _currentlyRunningThreadsCount; }
            set
            {
                if (value == _currentlyRunningThreadsCount) return;
                _currentlyRunningThreadsCount = value;
                OnPropertyChanged();
            }
        }
        public int CurrentScanningProgress
        {
            get { return _currentScanningProgress; }
            set
            {
                if (value == _currentScanningProgress) return;
                _currentScanningProgress = value;
                OnPropertyChanged();
            }
        }
        public AppState ApplicationState
        {
            get { return _applicationState; }
            set
            {
                if (value == _applicationState) return;
                _applicationState = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
