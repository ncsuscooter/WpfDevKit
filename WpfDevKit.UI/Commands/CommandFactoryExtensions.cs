﻿using System.Diagnostics;
using WpfDevKit.Busy;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.UI.Command
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit UI services.
    /// </summary>
    [DebuggerStepThrough]
    public static class CommandFactoryExtensions
    {
        /// <summary>
        /// Registers the <see cref="ICommandFactory"/> and <see cref="IBusyService"/> services.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddCommandFactory(this IServiceCollection services) =>
            services.AddSingleton<CommandFactory>()
                    .AddSingleton<ICommandFactory>(p => p.GetRequiredService<CommandFactory>());
    }
}
