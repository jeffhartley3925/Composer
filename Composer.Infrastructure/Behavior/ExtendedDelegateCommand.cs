using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Infrastructure.Behavior
{
    public class ExtendedDelegateCommand<T> : ICommand
    {
        private readonly Action<T> executeAction;
        private readonly Func<T, bool> canExecuteAction;

        public ExtendedDelegateCommand(Action<T> executeAction)
        {
            this.executeAction = executeAction;
        }
        public ExtendedDelegateCommand(Action<T> executeAction, Func<T, bool> canExecuteAction)
        {
            this.executeAction = executeAction;
            this.canExecuteAction = canExecuteAction;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            executeAction((T)parameter);
        }
    }
}
