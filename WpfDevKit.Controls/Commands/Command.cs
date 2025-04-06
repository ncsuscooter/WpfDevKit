using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Represents a command that can be executed with a parameter of type <typeparamref name="T"/>.
    /// Implements <see cref="ICommand"/> and allows for execution based on a condition.
    /// </summary>
    [DebuggerStepThrough]
    internal class Command<T> : ICommand
    {
        private readonly Action<T> action;
        private readonly Predicate<object> canExecute;

        /// <summary>
        /// Occurs when the <see cref="ICommand.CanExecute"/> changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}"/> class.
        /// </summary>
        /// <param name="action">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate that determines if the command can be executed.</param>
        public Command(Action<T> action, Predicate<object> canExecute = default) => (this.action, this.canExecute) = (action, canExecute);

        /// <summary>
        /// Determines whether the command can be executed.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the <see cref="CanExecute"/> predicate.</param>
        /// <returns>True if the command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter) => canExecute is null || canExecute(parameter);

        /// <summary>
        /// Executes the command with the provided parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the <see cref="Execute"/> action.</param>
        public void Execute(object parameter) => action?.Invoke((T)parameter);
    }
}
