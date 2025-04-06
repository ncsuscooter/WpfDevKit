using System.Collections.Concurrent;
using System.Windows;

namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Manages dialog window ownership context within the application.
    /// Tracks open dialog windows in a LIFO stack to ensure proper modal ownership.
    /// </summary>
    internal static class DialogWindowContext
    {
        private static readonly ConcurrentStack<Window> windowStack = new ConcurrentStack<Window>();

        /// <summary>
        /// Pushes a window onto the internal context stack.
        /// Should be called when a dialog window is initialized or shown.
        /// </summary>
        /// <param name="window">The dialog window to track.</param>
        public static void Push(Window window)
        {
            if (window != null)
                windowStack.Push(window);
        }

        /// <summary>
        /// Pops the most recent window off the context stack.
        /// Should be called when a dialog window is closed.
        /// </summary>
        public static void Pop() => windowStack.TryPop(out _);

        /// <summary>
        /// Gets the current top-most window to be used as the owner for new dialogs.
        /// Returns the application's main window if no dialogs are currently tracked.
        /// </summary>
        /// <returns>The current owner window, or the main window.</returns>
        public static Window GetCurrentOwner() => windowStack.TryPeek(out var top) ? top : Application.Current?.MainWindow;
    }
}
