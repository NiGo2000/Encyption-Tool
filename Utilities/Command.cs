using System;
using System.Windows.Input;

namespace Encrypter.Utilities
{
    /// <summary>
    /// An always executable command
    /// </summary>
    public class Command : ICommand
    {
        private readonly Action actionReadonly;

        /// <summary>
        /// Produces a command that can always be executed
        /// </summary>
        /// <param name="action">The method to be executed</param>
        public Command(Action action)
        {
            actionReadonly = action;
        }

        public void Execute(object parameter)
        {
            actionReadonly?.Invoke();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
