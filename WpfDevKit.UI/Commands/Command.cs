using System;
using System.Diagnostics;
using WpfDevKit.UI.Synchronization.Context;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Represents a non-generic synchronous command for use in data bindings.
    /// Inherits from <see cref="Command{Object}"/>.
    /// </summary>
    [DebuggerStepThrough]
    internal class Command : Command<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="contextService">A service that provides the application's UI context.</param>
        /// <param name="execute">The action to invoke when the command is executed.</param>
        /// <param name="canExecute">An optional predicate to determine if the command can execute.</param>
        public Command(IContextSynchronizationService contextService, Action<object> execute, Predicate<object> canExecute = null) : base(contextService, execute, canExecute) { }
    }

    /// <summary>
    /// Represents a synchronous command that executes an <see cref="Action{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    [DebuggerStepThrough]
    internal class Command<T> : CommandBase<T>
    {
        private readonly Action<T> execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}"/> class.
        /// </summary>
        /// <param name="contextService">A service that provides the application's UI context.</param>
        /// <param name="execute">The action to invoke when the command is executed.</param>
        /// <param name="canExecute">An optional predicate to determine if the command can execute.</param>
        public Command(IContextSynchronizationService contextService, Action<T> execute, Predicate<T> canExecute = default) : base(contextService, canExecute) => this.execute = execute;

        /// <summary>
        /// Executes the command using the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the action.</param>
        public override void Execute(T parameter) => execute?.Invoke(parameter);
    }
}
