using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// The default factory implementation for creating <see cref="ICommand"/> instances.
    /// </summary>
    [DebuggerStepThrough]
    internal class CommandFactory : ICommandFactory
    {
        /// <inheritdoc/>
        public ICommand GetCommand<T>(Action<T> action, Predicate<object> predicate = default) => new Command<T>(action, predicate);
    }
}
