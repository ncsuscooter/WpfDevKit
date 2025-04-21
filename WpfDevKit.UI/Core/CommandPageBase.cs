using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfDevKit.Busy;
using WpfDevKit.Logging;
using WpfDevKit.UI.Command;

namespace WpfDevKit.UI.Core
{
    /// <summary>
    /// Provides an abstract base class for commands that perform actions asynchronously.
    /// Inherits from <see cref="ObservableBase"/> and exposes a <see cref="ICommand"/> for command execution.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class CommandPageBase : ObservableBase, IDisposable
    {
        private readonly Dictionary<string, Func<CancellationToken, Task>> commandActions = new Dictionary<string, Func<CancellationToken, Task>>();
        private readonly IBusyService busyService;
        private readonly ICommandFactory commandFactory;
        private readonly ILogService logService;
        protected bool isDisposed;

        /// <summary>
        /// Gets the command that executes <see cref="DoCommandAsync"/> with a string parameter.
        /// </summary>
        public ICommand Command => commandFactory.CreateAsyncCommand<string>(async (command, token) => await DoCommandAsync(command));

        /// <summary>
        /// 
        /// </summary>
        public bool IsBusy => busyService.IsBusy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPageBase"/> class with the specified command factory.
        /// Additionally accepts an instance of <see cref="IBusyService"/> to indicate use of <see cref="IAsyncCommand{T}"/> commands.
        /// </summary>
        /// <param name="busyService">The <see cref="IBusyService"/> used to indicate background activity.</param>
        /// <param name="commandFactory">The <see cref="ICommandFactory"/> used to create commands.</param>
        /// <param name="logService">The <see cref="ILogService"/> used to log messages and exceptions.</param>
        public CommandPageBase(IBusyService busyService, ICommandFactory commandFactory, ILogService logService)
        {
            this.busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            this.commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
            this.busyService.IsBusyChanged += OnBusyServiceIsBusyChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RegisterCommand(string command, Func<CancellationToken, Task> action)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));
            commandActions[command] = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void RegisterCommand(string command, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            RegisterCommand(command, _ =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Disposes the command page and cleans up any resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (isDisposed)
                return;
            logService.LogDebug(type: GetType());
            try
            {
                busyService.IsBusyChanged -= OnBusyServiceIsBusyChanged;
            }
            finally
            {
                isDisposed = true;
            }
        }

        /// <summary>
        /// Abstract method that should be implemented to perform the asynchronous command action.
        /// </summary>
        /// <param name="commandName">The name of the command to be executed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DoCommandAsync(string commandName, CancellationToken cancellationToken = default)
        {
            logService.LogTrace(null, $"{nameof(commandName)}='{commandName}'", GetType());
            using (busyService.Busy())
            {
                await Task.Delay(50);
                if (commandActions.TryGetValue(commandName, out var action))
                    await action.Invoke(cancellationToken);
                else
                    logService.LogWarning("No action specified for the command provided");
            }
        }

        private void OnBusyServiceIsBusyChanged() => OnPropertyChanged(nameof(IsBusy));
    }
}
