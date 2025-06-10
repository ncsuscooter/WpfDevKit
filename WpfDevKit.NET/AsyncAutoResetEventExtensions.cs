using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit
{
    [DebuggerStepThrough]
    public static class AsyncAutoResetEventExtensions
    {
        /// <summary>
        /// Waits until the event is signaled, the timeout elapses, or the cancellationToken is canceled.
        /// Returns true if the event was signaled; false if the timeout expired.
        /// Throws if cancellationToken is canceled.
        /// </summary>
        public static async Task<bool> WaitAsync(this AsyncAutoResetEvent reset, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            // Create a linked token so we can cancel the delay if the event fires first
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var delayTask = Task.Delay(timeout, cts.Token);
                var waitTask = reset.WaitAsync(cts.Token);
                var completed = await Task.WhenAny(waitTask, delayTask).ConfigureAwait(false);
                if (completed == waitTask)
                {
                    // auto reset event fired before timeout => cancel the delay and return true
                    cts.Cancel();
                    await waitTask.ConfigureAwait(false); // propagate exceptions if any
                    return true;
                }
                // timeout won, return false
                return false;
            }
        }
    }
}