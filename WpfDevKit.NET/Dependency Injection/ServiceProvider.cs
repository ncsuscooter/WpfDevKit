using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WpfDevKit.Factory;

namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Provides resolution for services registered in a <see cref="ServiceCollection"/>.
    /// Supports constructor injection, property injection via the <see cref="ResolvableAttribute"/>.
    /// Also detects circular dependencies and supports resolution of multiple implementations using <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal class ServiceProvider : IServiceProvider
    {
        private readonly List<ServiceDescriptor> descriptors;
        private readonly ObjectFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class using the specified descriptors.
        /// </summary>
        /// <param name="descriptors">The collection of registered services to use for resolution.</param>
        public ServiceProvider(List<ServiceDescriptor> descriptors) => (this.descriptors, factory) = (descriptors, new ObjectFactory(this));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when circular dependencies are detected or when required constructor parameters cannot be resolved.
        /// </exception>
        /// <remarks>
        /// Behavior Details:<br/>
        /// - If multiple registrations exist for the same service type, the most recent registration is used (last one wins).<br/>
        /// - If <paramref name="serviceType"/> is <c>IEnumerable&lt;T&gt;</c>, all matching services are returned as an array.<br/>
        /// - Circular dependency detection is enforced and will throw if encountered.<br/>
        /// </remarks>
        public object GetService(Type serviceType) => GetService(serviceType, new HashSet<Type>());

        /// <summary>
        /// Internal resolution logic with circular dependency tracking.
        /// </summary>
        /// <param name="serviceType">The type of the service to resolve.</param>
        /// <param name="stack">Tracking stack to detect circular dependencies during recursive resolution.</param>
        /// <returns>The resolved instance, or <c>null</c> if the type is unregistered.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a circular dependency is detected or when constructor parameters cannot be resolved.
        /// </exception>
        private object GetService(Type serviceType, HashSet<Type> stack)
        {
            // Handle IEnumerable<T> resolution
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                var instances = descriptors.Where(d => d.ServiceType == elementType).Select(d => GetService(d.ServiceType, stack)).ToArray();
                var array = Array.CreateInstance(elementType, instances.Length);
                instances.CopyTo(array, 0);
                return array;
            }
            // Find last-registered descriptor
            var descriptor = descriptors.LastOrDefault(x => x.ServiceType == serviceType);
            if (descriptor == null)
                return null;
            // Handle singleton instance
            if (descriptor.Lifetime == TServiceLifetime.Singleton)
            {
                if (descriptor.Instance == null)
                    descriptor.Instance = CreateInstance(descriptor, stack);
                return descriptor.Instance;
            }
            // Always create new instance for transient
            return CreateInstance(descriptor, stack);
        }

        /// <summary>
        /// Creates a service instance using constructor injection or a custom factory delegate.
        /// </summary>
        /// <param name="descriptor">The service descriptor that defines how to create the service.</param>
        /// <param name="stack">Stack used for tracking nested resolutions and preventing circular dependencies.</param>
        /// <returns>The created service instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when constructor selection fails, required parameters are missing, or circular resolution is detected.
        /// </exception>
        private object CreateInstance(ServiceDescriptor descriptor, HashSet<Type> stack)
        {
            if (descriptor.Factory != null)
                return descriptor.Factory(this);
            return factory.Create(descriptor.ImplementationType, stack, null, () =>
            {
                var ctors = descriptor.ImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                if (ctors.Length != 1)
                    throw new InvalidOperationException($"Type '{descriptor.ImplementationType.Name}' must have exactly one public constructor. Found: {ctors.Length}.");
                return ctors[0];
            });
        }
    }
}
