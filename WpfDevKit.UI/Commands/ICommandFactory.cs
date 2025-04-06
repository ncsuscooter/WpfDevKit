using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Provides a factory method for creating <see cref="ICommand"/> and <see cref="IAsyncCommand{T}"/> instances.
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
        ICommand CreateCommand<T>(Action<T> action, Predicate<object> predicate = default);

        /// <summary>
        /// Creates a new <see cref="IAsyncCommand{T}"/> instance with the specified action and predicate.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="action">The asynchronous action to execute when the command is invoked.</param>
        /// <param name="predicate">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A new <see cref="IAsyncCommand{T}"/> instance.</returns>
        IAsyncCommand<T> CreateAsyncCommand<T>(Func<T, CancellationToken, Task> action, Predicate<T> predicate = default);

        /// <summary>
        /// Creates a new <see cref="IAsyncCommand{T}"/> instance with the specified action and predicate.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="action">The asynchronous action to execute when the command is invoked.</param>
        /// <param name="predicate">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A new <see cref="IAsyncCommand{T}"/> instance.</returns>
        IAsyncCommand<T> CreateBusyAsyncCommand<T>(Func<T, CancellationToken, Task> action, Predicate<T> predicate = default);
    }
}
