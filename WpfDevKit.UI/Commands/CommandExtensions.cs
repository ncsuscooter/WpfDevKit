using System;
using System.Windows.Input;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Provides additional helpers for cancellable async commands.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Cancels the command if it supports cancellation.
        /// </summary>
        public static void CancelIfSupported(this ICommand command)
        {
            if (command is IDisposable disposable)
                disposable.Dispose();
        }

        /// <summary>
        /// Indicates whether the command supports cancellation.
        /// </summary>
        public static bool SupportsCancellation(this ICommand command) => command is IDisposable;

        /// <summary>
        /// Forces the <see cref="CommandManager"/> to raise the <see cref="CommandManager.RequerySuggested"/> event.
        /// </summary>
        /// <param name="command">The command on which to raise the event</param>
        public static void RaiseCanExecuteChanged(this ICommand command) => (command as Command)?.RaiseCanExecuteChanged();
    }
}
