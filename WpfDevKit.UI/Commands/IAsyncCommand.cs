using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <inheritdoc/>
    public interface IAsyncCommand : IAsyncCommand<object>
    {

    }

    /// <summary>
    /// Defines a command that executes an asynchronous operation with a parameter of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of parameter passed to the command.</typeparam>
    public interface IAsyncCommand<T> : ICommand, IDisposable
    {
        /// <summary>
        /// Executes the asynchronous command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(T parameter);

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        bool CanExecute(T parameter);

        /// <summary>
        /// Requests cancellation of the currently running command.
        /// </summary>
        void Cancel();
    }

}
