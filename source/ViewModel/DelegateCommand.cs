using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CANBUSViewerInterface.ViewModel
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> executeMethod = null;
        private readonly Func<object, bool> canExecuteMethod = null;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested += value;
            }
        }


        /// <summary>
        /// construttore 
        /// </summary>
        /// <param name="executeMethod"></param>
        /// <param name="canExecuteMethod"></param>
        public DelegateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }


        public bool CanExecute(object parameter)
        {
            if (canExecuteMethod == null) return true;
            return this.canExecuteMethod(parameter);
        }

        public void Execute(object parameter)
        {
            if (executeMethod == null) return;
            this.executeMethod(parameter);
        }
    }
}