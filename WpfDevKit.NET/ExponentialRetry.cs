using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit
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
                var interval = CalculateDelay(count++, minimum, maximum, 100, 2.5);
                result = await function($"Retrying in {interval.ToReadableTime()} after [{count}] failed attempts");
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

        /// <summary>
        /// Calculates an exponential backoff delay using a fixed exponential growth curve.
        /// Delay grows rapidly and is bounded by the specified minimum and maximum values.
        /// </summary>
        /// <param name="attempt">The current retry attempt number (1-based).</param>
        /// <param name="min">The minimum delay in milliseconds. Used when <paramref name="attempt"/> is 1.</param>
        /// <param name="max">The maximum delay in milliseconds. Delay will never exceed this value.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the calculated backoff delay.</returns>

        public static TimeSpan CalculateDelay(int attempt, int min, int max) =>
            TimeSpan.FromMilliseconds(Math.Min(max, Math.Max(min, (int)Math.Round(Math.Exp(attempt / 10.0) * 1000.0, 0))));

        /// <summary>
        /// Calculates a smooth, bounded exponential backoff delay using a customizable curve and scaling.
        /// This version gives full control over growth curve shape and when the delay reaches its maximum.
        /// </summary>
        /// <param name="attempt">The current retry attempt number (1-based).</param>
        /// <param name="min">The minimum delay in milliseconds. Used at the first attempt.</param>
        /// <param name="max">The maximum delay in milliseconds. Delay will approach this as <paramref name="attempt"/> nears <paramref name="scale"/>.</param>
        /// <param name="scale">The number of attempts over which the delay will grow from minimum to maximum.</param>
        /// <param name="exponent">The exponential curve factor (e.g. 2.0 for quadratic, 3.0 for cubic growth).</param>
        /// <returns>A <see cref="TimeSpan"/> representing the calculated backoff delay.</returns>
        public static TimeSpan CalculateDelay(int attempt, int min, int max, int scale, double exponent) =>
            TimeSpan.FromMilliseconds(min + (int)((max - min) * Math.Pow(Math.Min(1.0, Math.Max(1.0, attempt) / Math.Max(1.0, scale)), exponent)));
    }
}