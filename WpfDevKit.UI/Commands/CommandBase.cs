using System;
using System.Diagnostics;
using System.Windows.Input;
using WpfDevKit.UI.Synchronization.Context;

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
        private readonly IContextSynchronizationService contextService;
        private readonly Predicate<T> canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase{T}"/> class.
        /// </summary>
        /// <param name="contextService">A service that provides the application's UI context.</param>
        /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
        protected CommandBase(IContextSynchronizationService contextService, Predicate<T> canExecute = null)
        {
            this.contextService = contextService ?? throw new ArgumentNullException(nameof(contextService));
            this.canExecute = canExecute;
        }

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
        /// Notifies WPF that the result of <see cref="ICommand.CanExecute(object)"/>
        /// may have changed, so any bound controls should re-query and update their enabled state.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            // make sure we’re on the UI thread
            if (!contextService.IsSynchronized)
                contextService.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
            else
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        // ICommand implementation

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            // no predicate? always executable
            if (canExecute == null)
                return true;

            // if it’s the right type, run it
            if (parameter is T t)
                return canExecute(t);

            // handle the “null parameter, T is a ref-type” case
            if (parameter == null && default(T) == null)
                return canExecute(default);

            // otherwise, refuse
            return false;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if ((this as ICommand).CanExecute(parameter))
                Execute((T)parameter);
        }
    }
}
