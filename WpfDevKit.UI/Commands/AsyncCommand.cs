using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Represents a non-generic asynchronous command with cancellation support.
    /// Inherits from <see cref="AsyncCommand{Object}"/>.
    /// </summary>
    [DebuggerStepThrough]
    internal class AsyncCommand : AsyncCommand<object>, IAsyncCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand"/> class.
        /// </summary>
        /// <param name="execute">The asynchronous operation to execute.</param>
        /// <param name="canExecute">An optional predicate to determine if the command can execute.</param>
        public AsyncCommand(Func<object, CancellationToken, Task> execute, Predicate<object> canExecute = null) : base(execute, canExecute) { }
    }

    /// <summary>
    /// Represents an asynchronous command with support for cancellation and typed parameters.
    /// Prevents concurrent execution and integrates with WPF's command infrastructure.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    [DebuggerStepThrough]
    internal class AsyncCommand<T> : Command<T>, IAsyncCommand<T>
    {
        private readonly Func<T, CancellationToken, Task> asyncExecute;
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Gets a value indicating whether the command is currently executing.
        /// </summary>
        public bool IsExecuting { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand{T}"/> class.
        /// </summary>
        /// <param name="asyncExecute">The asynchronous action to execute with cancellation support.</param>
        /// <param name="canExecute">Optional predicate to determine if the command can execute.</param>
        public AsyncCommand(Func<T, CancellationToken, Task> asyncExecute, Predicate<T> canExecute = null) : base(null, canExecute) =>
            this.asyncExecute = asyncExecute ?? throw new ArgumentNullException(nameof(asyncExecute));

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// Returns <c>false</c> if the command is already executing.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        public override bool CanExecute(T parameter) => !IsExecuting && base.CanExecute(parameter);

        /// <summary>
        /// Executes the asynchronous command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the async delegate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(T parameter)
        {
            if (!CanExecute(parameter))
                return;

            IsExecuting = true;
            RaiseCanExecuteChanged();

            using (cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    await asyncExecute(parameter, cancellationTokenSource.Token);
                }
                finally
                {
                    IsExecuting = false;
                    cancellationTokenSource = null;
                    RaiseCanExecuteChanged();
                }
            }
        }

        /// <inheritdoc/>
        public override void Execute(T parameter) => _ = ExecuteAsync(parameter); // Fire and forget

        /// <summary>
        /// Cancels the currently running operation, if one is active.
        /// </summary>
        public void Cancel() => cancellationTokenSource?.Cancel();

        /// <summary>
        /// Disposes the active cancellation token source, if one exists.
        /// </summary>
        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            GC.SuppressFinalize(this);
        }
    }
}
