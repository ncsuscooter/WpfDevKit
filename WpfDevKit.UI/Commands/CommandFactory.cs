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
        public CommandFactory(IBusyService busyService) => this.busyService = busyService;

        /// <inheritdoc/>
        public ICommand CreateCommand<T>(Action<T> execute, Predicate<T> canExecute = default) => new Command<T>(execute, canExecute);
        
        /// <inheritdoc/>
        public ICommand CreateCommand(Action<object> execute, Predicate<object> canExecute = null) => CreateCommand<object>(execute, canExecute);

        /// <inheritdoc/>
        public IAsyncCommand<T> CreateAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null) => new AsyncCommand<T>(execute, canExecute);

        /// <inheritdoc/>
        public IAsyncCommand CreateAsyncCommand(Func<object, CancellationToken, Task> execute, Predicate<object> canExecute = null) => new AsyncCommand(execute, canExecute);

        /// <inheritdoc/>
        public IAsyncCommand<T> CreateBusyAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null) =>
            new AsyncCommand<T>(
                async (parameter, token) =>
                {
                    using (busyService.Busy())
                        await execute(parameter, token);
                },
                canExecute);

        /// <inheritdoc/>
        public IAsyncCommand CreateBusyAsyncCommand(Func<object, CancellationToken, Task> execute, Predicate<object> canExecute = null) =>
            new AsyncCommand(
                async (parameter, token) =>
                {
                    using (busyService.Busy())
                        await execute(parameter, token);
                },
                canExecute);
    }
}
