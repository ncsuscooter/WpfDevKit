using System;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Provides a factory method for creating <see cref="ICommand"/> instances.
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Creates a new <see cref="ICommand"/> instance with the specified action and predicate.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="action">The action to execute when the command is invoked.</param>
        /// <param name="predicate">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A new <see cref="ICommand"/> instance.</returns>
        ICommand GetCommand<T>(Action<T> action, Predicate<object> predicate = default);
    }
}
