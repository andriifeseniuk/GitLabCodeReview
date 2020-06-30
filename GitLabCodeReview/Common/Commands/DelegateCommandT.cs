using System;
using System.Windows.Input;

namespace GitLabCodeReview.Common.Commands
{
    public class DelegateCommand<T> : ICommand
    {
        private readonly Func<T, bool> canExecuteMethod;
        private readonly Action<T> executeMethod;

        public DelegateCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            executeMethod = execute;
            canExecuteMethod = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteMethod == null)
            {
                return true;
            }

            return canExecuteMethod((T)parameter);
        }

        public void Execute(object parameter)
        {
            executeMethod((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
