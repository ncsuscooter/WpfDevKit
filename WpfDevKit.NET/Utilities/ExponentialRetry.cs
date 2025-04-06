using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WpfDevKit.Extensions;

namespace WpfDevKit.Utilities
{
    /// <summary>
    /// Provides a utility method for executing an asynchronous function with retry logic, implementing an exponential backoff strategy between retries.
    /// </summary>
    [DebuggerStepThrough]
    public static class ExponentialRetry
    {
        /// <summary>
        /// Executes an asynchronous function with retry logic, implementing exponential backoff between retries.
        /// </summary>
        /// <param name="function">The asynchronous function to execute. It receives a retry message and returns a Task with a boolean result indicating success.</param>
        /// <param name="minimum">The minimum retry interval in milliseconds. Must be greater than zero.</param>
        /// <param name="maximum">The maximum retry interval in milliseconds. Must be greater than zero.</param>
        /// <param name="token">A CancellationToken to allow the operation to be canceled.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minimum"/> or <paramref name="maximum"/> is less than or equal to zero.</exception>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static async Task ExecuteAsync(Func<string, Task<bool>> function, int minimum, int maximum, CancellationToken token)
        {
            if (minimum <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimum));
            if (maximum <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximum));
            int count = 0;
            bool result;
            do
            {
                var interval = Math.Min(maximum, Math.Max(minimum, (int)Math.Round(Math.Exp(count++ / 10.0) * 1000.0, 0)));
                result = await function($"Retrying in {TimeSpan.FromMilliseconds(interval).ToReadableTime()} after [{count}] failed attempts");
                if (!result)
                {
                    try
                    {
                        await Task.Delay(interval, token);
                    }
                    catch (OperationCanceledException)
                    {
                        // INTENTIONALLY LEFT EMPTY
                    }
                }
            } while (!token.IsCancellationRequested && !result);
        }
    }
}
