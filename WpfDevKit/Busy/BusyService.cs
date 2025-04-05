using System;
using System.Diagnostics;
using System.Threading;
using WpfDevKit.Utilities;

namespace WpfDevKit.Busy
{
    /// <summary>
    /// Manages a busy state using a counter to track active operations. Notifies subscribers when the busy state changes.
    /// </summary>
    [DebuggerStepThrough]
    internal class BusyService : IBusyService
    {
        private long busy;

        /// <inheritdoc/>
        public event Action IsBusyChanged;

        /// <inheritdoc/>
        public bool IsBusy => Interlocked.Read(ref busy) > 0;

        /// <inheritdoc/>
        public IDisposable Busy() => new StartStopRegistration(() =>
        {
            if (Interlocked.Increment(ref busy) == 1)
                IsBusyChanged?.Invoke();
        }, x =>
        {
            if (Interlocked.Decrement(ref busy) == 0)
                IsBusyChanged?.Invoke();
        });
    }
}
