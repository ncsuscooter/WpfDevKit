using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfDevKit.Busy;
using WpfDevKit.UI.Synchronization.Context;

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
        private readonly IContextSynchronizationService contextService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandFactory"/> class.
        /// </summary>
        /// <param name="busyService">The busy service used to track asynchronous command execution.</param>
        /// <param name="contextService">A service that provides the application's UI context.</param>
        public CommandFactory(IBusyService busyService, IContextSynchronizationService contextService)
        {
            this.busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            this.contextService = contextService ?? throw new ArgumentNullException(nameof(contextService));
        }

        /// <inheritdoc/>
        public ICommand CreateCommand<T>(Action<T> execute, Predicate<T> canExecute = default) => new Command<T>(contextService, execute, canExecute);
        
        /// <inheritdoc/>
        public ICommand CreateCommand(Action<object> execute, Predicate<object> canExecute = null) => new Command(contextService, execute, canExecute);

        /// <inheritdoc/>
        public IAsyncCommand<T> CreateAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null) => new AsyncCommand<T>(contextService, execute, canExecute);

        /// <inheritdoc/>
        public IAsyncCommand CreateAsyncCommand(Func<object, CancellationToken, Task> execute, Predicate<object> canExecute = null) => new AsyncCommand(contextService, execute, canExecute);

        /// <inheritdoc/>
        public IAsyncCommand<T> CreateBusyAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = null) =>
            new AsyncCommand<T>(contextService,
                async (parameter, token) =>
                {
                    using (busyService.Busy())
                        await execute(parameter, token);
                },
                canExecute);

        /// <inheritdoc/>
        public IAsyncCommand CreateBusyAsyncCommand(Func<object, CancellationToken, Task> execute, Predicate<object> canExecute = null) =>
            new AsyncCommand(contextService,
                async (parameter, token) =>
                {
                    using (busyService.Busy())
                        await execute(parameter, token);
                },
                canExecute);
    }
}
