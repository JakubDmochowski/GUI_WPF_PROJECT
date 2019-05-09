using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EguiLab2
{
    class RelayCommand : ICommand
    {

        readonly Action<object> _execute = null;
        readonly Predicate<object> _canExecute = null;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _canExecute = canExecute;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RecheckCanExecute()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }

    }
}