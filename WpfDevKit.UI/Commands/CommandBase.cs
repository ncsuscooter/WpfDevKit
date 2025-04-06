using System;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Provides a base implementation for strongly typed commands.
    /// Supports execution control through <see cref="CanExecute(object)"/> and command notification.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public abstract class CommandBase<T> : ICommand
    {
        private readonly Predicate<object> predicate;

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase{T}"/> class.
        /// </summary>
        /// <param name="predicate">An optional predicate that determines whether the command can execute.</param>
        protected CommandBase(Predicate<object> predicate = null)
        {
            this.predicate = predicate;
        }

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter passed from the command binding.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        public virtual bool CanExecute(object parameter) =>
            predicate?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public abstract void Execute(object parameter);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to signal the command's availability may have changed.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
