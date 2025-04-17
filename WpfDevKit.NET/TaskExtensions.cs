using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for working with <see cref="Task"/> objects.
/// </summary>
[DebuggerStepThrough]
public static class TaskExtensions
{
    /// <summary>
    /// Awaits a <see cref="Task"/> while observing the specified <see cref="CancellationToken"/>.
    /// If cancellation is requested before the task completes, an <see cref="OperationCanceledException"/> is thrown.
    /// </summary>
    /// <param name="task">The task to await.</param>
    /// <param name="token">The cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WithCancellation(this Task task, CancellationToken token = default)
    {
        using (var reg = token.Register(() => throw new OperationCanceledException(token)))
            await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits a <see cref="Task{TResult}"/> while observing the specified <see cref="CancellationToken"/>.
    /// If cancellation is requested before the task completes, an <see cref="OperationCanceledException"/> is thrown.
    /// </summary>
    /// <typeparam name="T">The result type returned by the task.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <param name="token">The cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation that returns a result of type <typeparamref name="T"/>.</returns>
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken token = default)
    {
        using (var reg = token.Register(() => throw new OperationCanceledException(token)))
            return await task.ConfigureAwait(false);
    }
}
