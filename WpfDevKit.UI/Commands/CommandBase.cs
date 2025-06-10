using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Provides a base implementation for strongly typed commands.
    /// Supports execution control through <see cref="CanExecute(T)"/> and command notification.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    [DebuggerStepThrough]
    internal abstract class CommandBase<T> : ICommand
    {
        private readonly Predicate<T> canExecute;

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
        /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
        protected CommandBase(Predicate<T> canExecute = null) => this.canExecute = canExecute;

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter passed from the command binding.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        public virtual bool CanExecute(T parameter) => canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public abstract void Execute(T parameter);

        /// <summary>
        /// Forces the <see cref="CommandManager"/> to raise the <see cref="CommandManager.RequerySuggested"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

        // ICommand implementation

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter) => !(parameter is T t) || canExecute is null || canExecute(t);

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if ((this as ICommand).CanExecute(parameter))
                Execute((T)parameter);
        }
    }
}
