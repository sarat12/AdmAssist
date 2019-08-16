using AdmAssist.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AdmAssist.Interfaces;

namespace AdmAssist.Models
{
    public class MenuTreeNode : IMenuTreeNode
    {
        private string _name;
        public ObservableCollection<IMenuTreeNode> Children { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public MenuTreeNode()
        {
            Children = new ObservableCollection<IMenuTreeNode>();
            Name = "New SubMenu";
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
