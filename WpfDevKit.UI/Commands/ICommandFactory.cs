using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Provides a factory method for creating instances of <see cref="ICommand"/>, <see cref="IAsyncCommand"/>, and <see cref="IAsyncCommand{T}"/>.
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Creates a new <see cref="ICommand"/> instance with the specified action and predicate.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A new <see cref="ICommand"/> instance.</returns>
        ICommand CreateCommand(Action<object> execute, Predicate<object> canExecute = default);

        /// <summary>
        /// Creates a new <see cref="ICommand"/> instance with the specified action and predicate.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A new <see cref="ICommand"/> instance.</returns>
        ICommand CreateCommand<T>(Action<T> execute, Predicate<T> canExecute = default);

        /// <summary>
        /// Creates a new <see cref="IAsyncCommand"/> instance with the specified action and predicate.
        /// </summary>
        /// <param name="execute">The asynchronous action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A new <see cref="IAsyncCommand"/> instance.</returns>
        IAsyncCommand CreateAsyncCommand(Func<object, CancellationToken, Task> execute, Predicate<object> canExecute = default);

        /// <summary>
        /// Creates a new <see cref="IAsyncCommand{T}"/> instance with the specified action and predicate.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="execute">The asynchronous action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A new <see cref="IAsyncCommand{T}"/> instance.</returns>
        IAsyncCommand<T> CreateAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = default);

        /// <summary>
        /// Creates a new <see cref="IAsyncCommand"/> instance with the specified action and predicate.
        /// </summary>
        /// <param name="execute">The asynchronous action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A cancellable async command <see cref="IAsyncCommand{T}"/> with busy tracking.</returns>
        IAsyncCommand CreateBusyAsyncCommand(Func<object, CancellationToken, Task> execute, Predicate<object> canExecute = default);

        /// <summary>
        /// Creates a new <see cref="IAsyncCommand{T}"/> instance with the specified action and predicate.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="execute">The asynchronous action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate to determine whether the command can execute.</param>
        /// <returns>A cancellable async command <see cref="IAsyncCommand{T}"/> with busy tracking.</returns>
        IAsyncCommand<T> CreateBusyAsyncCommand<T>(Func<T, CancellationToken, Task> execute, Predicate<T> canExecute = default);
    }
}
