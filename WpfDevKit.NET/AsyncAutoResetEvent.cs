using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit
{

    /// <summary>
    /// A lightweight, thread-safe async auto-reset event for one-time signaling.
    /// </summary>
    /// <remarks>
    /// Only one waiter is released per <see cref="Signal"/> call. If <see cref="Signal"/> is called multiple times 
    /// before any <see cref="WaitAsync(CancellationToken)"/> call is made, only one signal is stored — all extra signals are ignored.
    /// This prevents signal accumulation or queueing, making it ideal for early-wake or change-notification scenarios.
    /// </remarks>
    [DebuggerStepThrough]
    public class AsyncAutoResetEvent
    {
        private readonly ConcurrentQueue<TaskCompletionSource<bool>> queue = new ConcurrentQueue<TaskCompletionSource<bool>>();
        private bool signaled;

        /// <summary>
        /// Asynchronously waits for the signal to be set. Returns immediately if already signaled.
        /// </summary>
        public async Task WaitAsync(CancellationToken cancellationToken = default)
        {
            if (signaled)
            {
                signaled = false;
                return;
            }
            var tcs = new TaskCompletionSource<bool>();
            queue.Enqueue(tcs);
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        /// <summary>
        /// Signals the event, releasing one waiter if any are currently waiting. If no waiters are present, stores one signal.
        /// </summary>
        public void Signal()
        {
            TaskCompletionSource<bool> toRelease = null;
            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out var tcs) && !tcs.Task.IsCanceled && !tcs.Task.IsCompleted)
                {
                    toRelease = tcs;
                    break;
                }
            }
            if (!signaled && toRelease == null)
                signaled = true;
            toRelease?.SetResult(true);
        }
    }
}