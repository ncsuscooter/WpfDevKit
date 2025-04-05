using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// Provides a base class for implementing long-running background services that support start and stop operations.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class BackgroundService : IBackgroundService
    {
        private Task task;
        private CancellationTokenSource cancellationTokenSource;

        /// <inheritdoc/>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (task != null)
                return Task.CompletedTask;
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            task = ExecuteAsync(cancellationTokenSource.Token);
            return task.IsCompleted ? task : Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (task == null)
                return;
            try
            {
                cancellationTokenSource.Cancel();
            }
            finally
            {
                var taskCompletionSource = new TaskCompletionSource<object>();
                using (var registration = cancellationToken.Register(s => ((TaskCompletionSource<object>)s).SetCanceled(), taskCompletionSource))
                    await Task.WhenAny(task, taskCompletionSource.Task).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose() => cancellationTokenSource?.Cancel();

        /// <summary>
        /// Executes the logic of the background service.
        /// This method is called when the service is started and must be implemented by derived classes.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the background operation.</returns>
        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
