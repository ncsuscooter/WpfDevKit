﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfDevKit.Busy;
using WpfDevKit.Logging;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.Synchronization.Context;

namespace WpfDevKit.UI.Core
{
    /// <summary>
    /// Provides an abstract base class for commands that perform actions asynchronously.
    /// Inherits from <see cref="ObservableBase"/> and exposes a <see cref="ICommand"/> for command execution.
    /// </summary>
    [DebuggerStepThrough]
    public class CommandPageBase : ObservableBase, IDisposable
    {
        /// <summary>
        /// The message to indicate that the base method should be overridden to prevent execution.
        /// </summary>
        protected const string OVERRIDE_MESSAGE = "Override the base method to prevent execution";

        private readonly Dictionary<string, Func<CancellationToken, Task>> commandActions = new Dictionary<string, Func<CancellationToken, Task>>();
        private readonly IBusyService busyService;
        private readonly ICommandFactory commandFactory;
        private readonly IContextSynchronizationService contextService;
        private readonly ILogService logService;
        private bool isDisposed;

        /// <summary>
        /// Gets the command that executes <see cref="DoCommandAsync"/> with a string parameter.
        /// </summary>
        public ICommand Command => commandFactory.CreateBusyAsyncCommand<string>((command, token) => DoCommandAsync(command));

        public bool IsBusy => busyService.IsBusy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPageBase"/> class with the specified command factory.
        /// Additionally accepts an instance of <see cref="IBusyService"/> to indicate use of <see cref="IAsyncCommand{T}"/> commands.
        /// </summary>
        /// <param name="busyService">The <see cref="IBusyService"/> used to indicate background activity.</param>
        /// <param name="commandFactory">The <see cref="ICommandFactory"/> used to create commands.</param>
        /// <param name="logService">The <see cref="ILogService"/> used to log messages and exceptions.</param>
        public CommandPageBase(IBusyService busyService, ICommandFactory commandFactory, IContextSynchronizationService contextService, ILogService logService)
            : base(logService)
        {
            this.busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            this.busyService.IsBusyChanged += OnBusyServiceIsBusyChanged;
            this.commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            this.contextService = contextService ?? throw new ArgumentNullException(nameof(contextService));
            this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
            this.logService.LogTrace(GetType());
        }

        protected void RegisterCommand(string command, Func<CancellationToken, Task> action)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));
            commandActions[command] = action ?? throw new ArgumentNullException(nameof(action));
        }

        protected void RegisterCommand(string command, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            RegisterCommand(command, _ =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        protected void UnregisterCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));
            if (commandActions.ContainsKey(command))
                commandActions.Remove(command);
        }

        protected void UnregisterCommands() => commandActions.Clear();

        /// <summary>
        /// Disposes the command page and cleans up any resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            logService.LogTrace(GetType());
            busyService.IsBusyChanged -= OnBusyServiceIsBusyChanged;
            UnregisterCommands();
            UnregisterPropertyChangingActions();
            UnregisterPropertyChangedActions();
        }

        /// <summary>
        /// Abstract method that should be implemented to perform the asynchronous command action.
        /// </summary>
        /// <param name="commandName">The name of the command to be executed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DoCommandAsync(string commandName, CancellationToken cancellationToken = default)
        {
            logService.LogTrace($"{nameof(commandName)}='{commandName}'", GetType());
            await Task.Delay(50);
            if (commandActions.TryGetValue(commandName, out var action))
                await action.Invoke(cancellationToken);
            else
                logService.LogWarning("No action specified for the command provided");
        }

        private void OnBusyServiceIsBusyChanged() => contextService.BeginInvoke(() => OnPropertyChanged(nameof(IsBusy)));
    }
}
