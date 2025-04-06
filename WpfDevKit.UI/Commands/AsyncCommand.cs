using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Represents an asynchronous command with support for cancellation and typed parameters.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    internal class AsyncCommand<T> : IAsyncCommand<T>
    {
        private readonly Func<T, CancellationToken, Task> execute;
        private readonly Predicate<T> canExecute;
        private CancellationTokenSource cancellationTokenSource;
        private bool isExecuting;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableAsyncCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The asynchronous action to execute with cancellation support.</param>
        /// <param name="canExecute">Optional predicate to determine if the command can execute.</param>
        public AsyncCommand(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? (_ => true);
        }

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        public bool CanExecute(T parameter) =>
            !isExecuting && (canExecute?.Invoke(parameter) ?? true);

        /// <summary>
        /// Executes the asynchronous command with the specified parameter.
        /// </summary>
        public async Task ExecuteAsync(T parameter)
        {
            if (!CanExecute(parameter))
                return;

            isExecuting = true;
            RaiseCanExecuteChanged();

            using (cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    await execute(parameter, cancellationTokenSource.Token);
                }
                finally
                {
                    isExecuting = false;
                    cancellationTokenSource = null;
                    RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Requests cancellation of the currently running command.
        /// </summary>
        public void Cancel() => cancellationTokenSource?.Cancel();

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        // ICommand members
        bool ICommand.CanExecute(object parameter) => parameter is T t && CanExecute(t);

        void ICommand.Execute(object parameter)
        {
            if (parameter is T t)
                _ = ExecuteAsync(t); // fire-and-forget for ICommand
        }

        /// <summary>
        /// Disposes the current cancellation token source if active.
        /// </summary>
        public void Dispose() => cancellationTokenSource?.Dispose();
    }

}
