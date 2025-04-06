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
    }

}
