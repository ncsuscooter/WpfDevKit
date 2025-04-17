using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
    private readonly object sync = new object();
    private TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool signaled;

    /// <summary>
    /// Asynchronously waits for the signal to be set. Returns immediately if already signaled.
    /// </summary>
    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (signaled)
            {
                signaled = false;
                return Task.FromResult(true);
            }
            else
            {
                return taskCompletionSource.Task.WithCancellation(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Signals the event, releasing one waiter if any are currently waiting. If no waiters are present, stores one signal.
    /// </summary>
    public void Signal()
    {
        lock (sync)
        {
            if (taskCompletionSource.Task.IsCompleted)
                taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!taskCompletionSource.Task.IsCompleted)
                taskCompletionSource.SetResult(true);
            else
                signaled = true;
        }
    }
}
