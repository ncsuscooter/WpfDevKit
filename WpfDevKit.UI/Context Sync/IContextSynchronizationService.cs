using System;

namespace WpfDevKit.UI.ContextSynchronization
{
    /// <summary>
    /// Provides methods to interact with the context in which actions are executed,
    /// such as ensuring synchronization and controlling the context's lifecycle.
    /// </summary>
    public interface IContextSynchronizationService
    {
        /// <summary>
        /// Gets a value indicating whether the context is synchronized.
        /// </summary>
        /// <value><c>true</c> if the context is synchronized; otherwise, <c>false</c>.</value>
        bool IsSynchronized { get; }

        /// <summary>
        /// Executes the specified action synchronously in the current context.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        void Invoke(Action action);

        /// <summary>
        /// Begins executing the specified action asynchronously in the current context.
        /// </summary>
        /// <param name="action">The action to begin executing.</param>
        void BeginInvoke(Action action);

        /// <summary>
        /// Shuts down the context, optionally specifying an exit code.
        /// </summary>
        /// <param name="exitCode">The exit code to return upon shutdown. If <c>null</c>, a default exit code is used.</param>
        void Shutdown(int? exitCode);
    }
}
