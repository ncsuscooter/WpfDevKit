using System;
using System.Diagnostics;

namespace WpfDevKit
{

    /// <summary>
    /// A utility class that measures the elapsed time between instantiation and disposal, executing optional start and stop actions.
    /// </summary>
    [DebuggerStepThrough]
    public class StartStopRegistration : IDisposable
    {
        private readonly Stopwatch stopwatch;
        private readonly Action<long> action;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartStopRegistration"/> class, starts a stopwatch, and optionally invokes a start action.
        /// </summary>
        /// <param name="startAction">An optional action to invoke when the instance is created.</param>
        /// <param name="stopAction">An optional action to invoke when the instance is disposed.  It receives the elapsed time in milliseconds.</param>
        public StartStopRegistration(Action startAction = default, Action<long> stopAction = default)
        {
            action = stopAction;
            stopwatch = Stopwatch.StartNew();
            startAction?.Invoke();
        }

        /// <summary>
        /// Stops the stopwatch and invokes the stop action, passing the elapsed time in milliseconds.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            stopwatch.Stop();
            action?.Invoke(stopwatch.ElapsedMilliseconds);
        }
    }
}