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

        public int Count => queue.Count;

        public async Task<bool> TryWaitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await WaitAsync(cancellationToken);
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        public async Task<bool> TryWaitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                return await WaitAsync(timeout, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

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
                await tcs.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously waits for the signal to be set within a specified timeout.
        /// </summary>
        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            // Create a linked token so we can cancel the delay if the event fires first
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var d = Task.Delay(timeout, cts.Token);
                var w = WaitAsync(cts.Token);
                var task = await Task.WhenAny(w, d).ConfigureAwait(false);
                if (task == w)
                {
                    cts.Cancel();
                    await w.ConfigureAwait(false); // propagate exceptions if any
                    return true;
                }
                return false;
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