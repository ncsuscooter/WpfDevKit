using System;
using System.Diagnostics;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Configuration options for the <see cref="MemoryLogProvider"/> class, which handles logging to memory.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class MemoryLogProviderOptions : ILogProviderOptions
    {
        private int capacity = 8196;
        private int fillFactor = 80;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <see cref="TLogCategory.None"/> bitwise complemented 
        /// (<c>~TLogCategory.None</c>), meaning all categories are enabled by default.
        /// </remarks>
        public TLogCategory EnabledCategories { get; set; } = ~TLogCategory.None;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <see cref="TLogCategory.None"/>, meaning no categories 
        /// are disabled by default.
        /// </remarks>
        public TLogCategory DisabledCategories => TLogCategory.None;

        /// <summary>
        /// The maximum number of log messages that can be stored in memory.
        /// This value must be a positive integer and cannot exceed <see cref="int.MaxValue"/>.
        /// The default value is 8196.
        /// </summary>
        public int Capacity
        {
            get => capacity;
            set => capacity = Math.Min(int.MaxValue, Math.Max(0, value));
        }

        /// <summary>
        /// The percentage of memory capacity to be used before older messages are removed.
        /// This value is clamped between 0 and 100. A fill factor of 100 means no messages are removed
        /// until the maximum capacity is reached, while a lower value removes messages earlier.
        /// The default value is 80 (80% of the capacity).
        /// </summary>
        public int FillFactor
        {
            get => fillFactor;
            set => fillFactor = Math.Min(100, Math.Max(0, value));
        }
    }
}
