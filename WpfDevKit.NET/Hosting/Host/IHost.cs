﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// A program abstraction.
    /// </summary>
    public interface IHost : IDisposable
    {
        /// <summary>
        /// Gets the services configured for the program.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Starts the <see cref="IHostedService" /> objects configured for the program.
        /// The application will run until interrupted or until <see cref="IHostLifetime.StopApplication" /> is called.
        /// </summary>
        /// <param name="cancellationToken">Used to abort program start.</param>
        /// <returns>A <see cref="Task"/> that will be completed when the <see cref="IHost"/> starts.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to gracefully stop the program.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns>A <see cref="Task"/> that will be completed when the <see cref="IHost"/> stops.</returns>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts all registered background services synchronously.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops all registered background services synchronously.
        /// </summary>
        void Stop();
    }
}
