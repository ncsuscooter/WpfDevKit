using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using WpfDevKit.Interfaces;

namespace WpfDevKit.Controls.Services
{
    /// <summary>
    /// Provides services for executing actions on the UI thread in WPF applications.
    /// This class helps ensure that UI updates and other tasks are executed on the correct thread (the UI thread).
    /// </summary>
    [DebuggerStepThrough]
    internal class ContextService : IContextService
    {
        /// <summary>
        /// Gets a value indicating whether the current thread is the UI thread.
        /// This property checks if the current thread is the same as the thread associated with the WPF application's dispatcher.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current thread is the UI thread; otherwise, <c>false</c>.
        /// </value>
        public bool IsSynchronized => Application.Current.Dispatcher.Thread == Thread.CurrentThread;

        /// <summary>
        /// Executes the specified action synchronously on the UI thread.
        /// This method ensures that the action is executed on the thread that is associated with the WPF application's dispatcher.
        /// </summary>
        /// <param name="action">The action to execute on the UI thread.</param>
        /// <remarks>
        /// This method calls <see cref="Dispatcher.Invoke(Action)"/> to run the provided action synchronously on the UI thread.
        /// If called from a non-UI thread, it will block the calling thread until the action is completed on the UI thread.
        /// </remarks>
        public void Invoke(Action action) => Application.Current.Dispatcher.Invoke(action);

        /// <summary>
        /// Executes the specified action asynchronously on the UI thread.
        /// This method ensures that the action is executed on the thread associated with the WPF application's dispatcher.
        /// </summary>
        /// <param name="action">The action to execute on the UI thread.</param>
        /// <remarks>
        /// This method calls <see cref="Dispatcher.BeginInvoke(Action)"/> to run the provided action asynchronously on the UI thread.
        /// The calling thread will not block while waiting for the action to complete.
        /// </remarks>
        public void BeginInvoke(Action action) => Application.Current.Dispatcher.BeginInvoke(action);

        /// <summary>
        /// Shuts down the WPF application and terminates the process with the specified exit code.
        /// </summary>
        /// <param name="exitCode">The exit code for the application. If not provided, the default exit code of 0 is used.</param>
        /// <remarks>
        /// This method calls <see cref="Application.Shutdown(int)"/> to gracefully shut down the application and optionally pass an exit code.
        /// </remarks>
        public void Shutdown(int? exitCode = default) => Application.Current.Shutdown(exitCode.GetValueOrDefault());
    }
}
