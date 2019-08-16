using AdmAssist.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AdmAssist.ViewModels
{
    public class IpRangeViewModel : INotifyPropertyChanged
    {
        private string _left;
        private string _right;

        public string Left
        {
            get { return _left; }
            set
            {
                if (value == _left) return;
                _left = value;
                OnPropertyChanged();
            }
        }

        public string Right
        {
            get { return _right; }
            set
            {
                if (value == _right) return;
                _right = value;
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
