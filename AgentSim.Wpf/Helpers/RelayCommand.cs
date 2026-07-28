using System;
using System.Windows.Input;

namespace AgentSim.Wpf.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
{
    if (_canExecute == null)
    {
        return true;
    }
    else
    {
        return _canExecute();
    }
}

public void Execute(object parameter)
{
    _execute();
}
    }
}