using System;
using System.Windows.Input;

namespace KogaSample
{
    public class TCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public TCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (this.canExecute == null)
                return true;
            else
            {
                return this.canExecute(parameter);
            }
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
