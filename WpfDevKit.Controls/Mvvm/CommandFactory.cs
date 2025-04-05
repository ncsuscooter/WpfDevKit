using System;
using System.Diagnostics;
using System.Windows.Input;
using WpfDevKit.Interfaces;

namespace WpfDevKit.Mvvm
{
    /// <summary>
    /// A factory implementation for creating <see cref="ICommand"/> instances.
    /// </summary>
    [DebuggerStepThrough]
    public class CommandFactory : ICommandFactory
    {
        /// <inheritdoc/>
        public ICommand GetCommand<T>(Action<T> action, Predicate<object> predicate = default) => new Command<T>(action, predicate);
    }
}
