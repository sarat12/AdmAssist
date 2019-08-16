using AdmAssist.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AdmAssist.Models
{
    public class SelectableObject<T> : INotifyPropertyChanged
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public T ObjectData { get; set; }

        public SelectableObject(T objectData)
        {
            ObjectData = objectData;
        }

        public SelectableObject(T objectData, bool isSelected)
        {
            IsSelected = isSelected;
            ObjectData = objectData;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
