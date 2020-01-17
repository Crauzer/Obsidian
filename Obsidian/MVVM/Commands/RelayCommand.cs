using System;
using System.Windows.Input;

namespace Obsidian.MVVM.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> execute) : this(null, execute)
        {

        }

        public RelayCommand(Predicate<object> canExecute, Action<object> execute)
        {
            this._canExecute = canExecute;
            this._execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (this._canExecute != null)
            {
                return this._canExecute(parameter);
            }

            return true;
        }

        public void Execute(object parameter)
        {
            this._execute(parameter);
        }
    }
}
