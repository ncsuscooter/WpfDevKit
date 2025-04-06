using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfDevKit.Busy;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Default factory for creating <see cref="ICommand"/> and <see cref="IAsyncCommand{T}"/> instances.
    /// Supports optional integration with <see cref="IBusyService"/> and cancellation.
    /// </summary>
    [DebuggerStepThrough]
    internal class CommandFactory : ICommandFactory
    {
        private readonly IBusyService busyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandFactory"/> class.
        /// </summary>
        /// <param name="busyService">The busy service used to track asynchronous command execution.</param>
        public CommandFactory(IBusyService busyService) =>
            this.busyService = busyService;

        /// <inheritdoc/>
        public ICommand CreateCommand<T>(Action<T> action, Predicate<object> predicate = default) => new Command<T>(action, predicate);

        /// <inheritdoc/>
        public IAsyncCommand<T> CreateAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null) => new AsyncCommand<T>(execute, canExecute);

        /// <summary>
        /// Creates an <see cref="IAsyncCommand{T}"/> that wraps execution with <see cref="IBusyService"/>.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="execute">The asynchronous operation to execute.</param>
        /// <param name="canExecute">Predicate determining if the command can execute.</param>
        /// <returns>A cancellable async command with busy tracking.</returns>
        public IAsyncCommand<T> CreateBusyAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null) =>
            new AsyncCommand<T>(
                async (parameter, token) =>
                {
                    using (busyService.Busy())
                        await execute(parameter, token);
                },
                canExecute);
    }
}
