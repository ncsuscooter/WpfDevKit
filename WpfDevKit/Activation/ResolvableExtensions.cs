using System;
using System.Linq;
using System.Reflection;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Activation
{
    /// <summary>
    /// Provides extension methods for registering services and configuring the
    /// resolvable container within the application.
    /// </summary>
    public static class ResolvableExtensions
    {
        /// <summary>
        /// Scans the specified <see cref="Assembly"/> for classes marked with
        /// <see cref="ResolvableAttribute"/> and registers them as transient services.
        /// </summary>
        /// <param name="services">The service collection to add registrations to.</param>
        /// <param name="assembly">The assembly to scan for resolvable types.</param>
        /// <param name="log">Optional logging callback for reporting registered types.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddResolvableFromAssembly(this IServiceCollection services, Assembly assembly, Action<string> log = null) => 
            services.AddResolvableFromAssemblies(new[] { assembly }, log);

        /// <summary>
        /// Scans the specified assemblies for classes marked with
        /// <see cref="ResolvableAttribute"/> and registers them as transient services.
        /// </summary>
        /// <param name="services">The service collection to add registrations to.</param>
        /// <param name="assemblies">An array of assemblies to scan for resolvable types.</param>
        /// <param name="log">Optional logging callback for reporting registered types.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddResolvableFromAssemblies(this IServiceCollection services, Assembly[] assemblies, Action<string> log = null)
        {
            var collection = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ResolvableAttribute>() != null)
                .ToList();
            foreach (var item in collection)
            {
                services.AddTransient(item, item);
                log?.Invoke($"Object registered: {item.FullName}");
            }
            return services;
        }
    }
}
