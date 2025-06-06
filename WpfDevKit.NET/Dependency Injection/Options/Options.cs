﻿using System.Diagnostics;

namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Provides access to configured <typeparamref name="TOptions"/> instances.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    [DebuggerStepThrough]
    internal class Options<TOptions> : IOptions<TOptions> where TOptions : class
    {
        /// <summary>
        /// Gets the configured <typeparamref name="TOptions"/> instance.
        /// </summary>
        public TOptions Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Options{TOptions}"/> class.
        /// </summary>
        /// <param name="value">The configured options instance.</param>
        public Options(TOptions value) => Value = value;
    }
}
