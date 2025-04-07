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
    internal class Command : Command<object>
    {
        public Command(Action<object> action, Predicate<object> canExecute = null) : base(action, canExecute)
        {
        }
    }

    /// <summary>
    /// Represents a command that can be executed with a parameter of type <typeparamref name="T"/>.
    /// Implements <see cref="ICommand"/> and allows for execution based on a condition.
    /// </summary>
    [DebuggerStepThrough]
    internal class Command<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

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
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate that determines if the command can be executed.</param>
        public Command(Action<T> execute, Predicate<T> canExecute = default) => (this.execute, this.canExecute) = (execute, canExecute);

        /// <summary>
        /// Determines whether the command can be executed.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the <see cref="CanExecute"/> predicate.</param>
        /// <returns>True if the command can be executed; otherwise, false.</returns>
        public bool CanExecute(T parameter) => canExecute is null || canExecute(parameter);

        /// <summary>
        /// Executes the command with the provided parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the <see cref="Execute"/> action.</param>
        public void Execute(T parameter) => execute?.Invoke(parameter);

        // ICommand implementation

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter) => !(parameter is T t) || canExecute is null || canExecute(t);

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if ((this as ICommand).CanExecute(parameter))
                execute((T)parameter);
        }
    }
}
