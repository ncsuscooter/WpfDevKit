﻿using System;
using System.Diagnostics;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Hosting;

namespace WpfDevKit.Connectivity
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    [DebuggerStepThrough]
    public static class ConnectivityServiceExtensions
    {
        /// <summary>
        /// Registers the connectivity monitoring service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddConnectivityService(this IServiceCollection services, Action<ConnectivityServiceOptions> configure) =>
            services.AddSingleton<ConnectivityService>()
                    .AddSingleton<IConnectivityService>(p => p.GetRequiredService<ConnectivityService>())
                    .AddSingleton<IHostedService>(p => p.GetRequiredService<ConnectivityService>())
                    .AddOptions(configure);
    }
}
