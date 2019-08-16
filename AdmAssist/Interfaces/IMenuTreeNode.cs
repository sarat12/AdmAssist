using System.ComponentModel;

namespace AdmAssist.Interfaces
{
    public interface IMenuTreeNode : INotifyPropertyChanged
    {
        string Name { get; set; }
    }
}
