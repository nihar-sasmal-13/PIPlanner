using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PIPlanner.Helpers
{
    internal class Command : ICommand
    {
        protected bool _runOnUIThread = false;
        protected Action<object> _action;

        public event EventHandler CanExecuteChanged;

        public Command(Action<object> action = null, bool runOnUIThread = false)
        {
            _runOnUIThread = runOnUIThread;
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (_runOnUIThread)
                App.Current.Dispatcher.Invoke(_action, parameter);
            else
                await Task.Run(() => _action(parameter));
        }
    }
}
