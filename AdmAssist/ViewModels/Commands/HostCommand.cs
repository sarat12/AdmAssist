using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AdmAssist.Models;

namespace AdmAssist.ViewModels.Commands
{
    public class HostCommand : ICommand
    {
        private readonly Action<IEnumerable<NotifyDynamicDictionary>, HostCommandNode> _action;
        private readonly HostCommandNode _hostCommandNode;

        public HostCommand(Action<IEnumerable<NotifyDynamicDictionary>, HostCommandNode> action, HostCommandNode hostCommandNode)
        {
            _action = action;
            _hostCommandNode = hostCommandNode;
        }

        public void Execute(object parameter)
        {
            _action(((IList)parameter).Cast<NotifyDynamicDictionary>(), _hostCommandNode);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
