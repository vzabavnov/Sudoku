using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace Zabavnov.Sudoku
{
    class Command: ICommand
    {
        private bool _canExecute;
        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteFunc;
        public Command(Action executeAction, Func<bool> canExecuteFunc = null)
        {
            Contract.Requires(executeAction != null);

            _executeAction = executeAction;
            _canExecuteFunc = canExecuteFunc ?? (() => true);
            _canExecute = _canExecuteFunc();
        }

        public bool CanExecute(object parameter)
        {
            var r = _canExecuteFunc();
            if (r != _canExecute)
            {
                _canExecute = r;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return r;
        }

        public void Execute(object parameter)
        {
            _executeAction();
        }

        public event EventHandler CanExecuteChanged;
    }
}
