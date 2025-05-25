using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WpfDevKit
{

    /// <summary>
    /// Provides extension methods for the <see cref="TimeSpan"/> structure.
    /// </summary>
    [DebuggerStepThrough]
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Converts a <see cref="TimeSpan"/> to a human-readable string representation.
        /// The string includes the largest non-zero units such as days, hours, minutes, seconds, and milliseconds.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to convert.</param>
        /// <returns>A string representing the <see cref="TimeSpan"/> in a readable format.</returns>
        /// <example>
        /// <code>
        /// TimeSpan time = new TimeSpan(1, 2, 3, 4, 500);
        /// string readableTime = time.ToReadableTime();
        /// // Output: "1d 2h 3m 4s 500ms"
        /// </code>
        /// </example>
        public static string ToReadableTime(this TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
                return "0 ms";
            var parts = new List<string>();
            void add(long val, string unit) { if (val > 0) parts.Add(val + unit); }
            add(timeSpan.Days, "d");
            add(timeSpan.Hours, "h");
            add(timeSpan.Minutes, "m");
            add(timeSpan.Seconds, "s");
            add(timeSpan.Milliseconds, "ms");
            if (parts.Count == 0)
                add(timeSpan.Ticks, "ticks");
            return string.Join(" ", parts);
        }
    }
}