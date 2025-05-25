using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WpfDevKit
{

    /// <summary>
    /// Provides extension methods for resolving services from an <see cref="IServiceProvider"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public static T GetService<T>(this IServiceProvider provider) => (T)provider.GetService(typeof(T));

        /// <summary>
        /// Gets an array of services of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The resolved array of service instances.</returns>
        public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) => (IEnumerable<T>)provider.GetService(typeof(IEnumerable<T>));

        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public static object GetRequiredService(this IServiceProvider provider, Type type) => provider.GetService(type) ??
            throw new InvalidOperationException($"Service of type {type.Name} is not registered.");

        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public static T GetRequiredService<T>(this IServiceProvider provider) => (T)provider.GetRequiredService(typeof(T));
    }
}