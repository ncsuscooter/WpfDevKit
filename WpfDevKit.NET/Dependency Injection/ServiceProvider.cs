using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
    [DebuggerStepThrough]
    internal class ServiceProvider : IServiceProvider, IObjectResolver
    {
        private readonly ConcurrentDictionary<ServiceDescriptor, object> cache;
        private readonly List<ServiceDescriptor> descriptors;
        private readonly ObjectFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class using the specified descriptors.
        /// </summary>
        /// <param name="descriptors">The collection of registered services to use for resolution.</param>
        public ServiceProvider(List<ServiceDescriptor> descriptors) =>
            (this.descriptors, factory, cache) = (descriptors, new ObjectFactory(this), new ConcurrentDictionary<ServiceDescriptor, object>());

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
        public object GetService(Type serviceType, HashSet<Type> stack)
        {
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                var instances = descriptors.Where(d => d.ServiceType == elementType).Select(d => GetService(d, stack)).ToArray();
                var array = Array.CreateInstance(elementType, instances.Length);
                instances.CopyTo(array, 0);
                return array;
            }
            var descriptor = descriptors.LastOrDefault(x => x.ServiceType == serviceType);
            return descriptor == null ? null : GetService(descriptor, stack);
        }

        /// <summary>
        /// Resolves and returns a service instance from the provided descriptor, using singleton caching when applicable.
        /// </summary>
        /// <param name="descriptor">The service descriptor that defines the service type and lifetime.</param>
        /// <param name="stack">The current resolution stack used to detect circular dependencies.</param>
        /// <returns>The resolved service instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when circular dependency is detected, constructor selection fails, or required parameters cannot be resolved.
        /// </exception>
        private object GetService(ServiceDescriptor descriptor, HashSet<Type> stack) => 
            descriptor.Lifetime == TServiceLifetime.Singleton ? cache.GetOrAdd(descriptor, _ => CreateInstance(descriptor, stack)) : CreateInstance(descriptor, stack);

        /// <summary>
        /// Instantiates a new service instance using either a factory delegate or constructor injection.
        /// This method does not perform caching and should be used internally by resolution logic.
        /// </summary>
        /// <param name="descriptor">The service descriptor that defines how to instantiate the service.</param>
        /// <param name="stack">The resolution stack for detecting circular dependencies.</param>
        /// <returns>A newly constructed service instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when constructor selection fails, circular dependencies are detected, or required parameters cannot be resolved.
        /// </exception>
        private object CreateInstance(ServiceDescriptor descriptor, HashSet<Type> stack) =>
            descriptor.Factory != null ? descriptor.Factory(this) : factory.Create(descriptor.ImplementationType, stack, null, () =>
            {
                var ctors = descriptor.ImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                if (ctors.Length != 1)
                    throw new InvalidOperationException($"Type '{descriptor.ImplementationType.Name}' must have exactly one public constructor. Found: {ctors.Length}.");
                return ctors[0];
            });

        /// <inheritdoc/>
        bool IObjectResolver.CanResolve(Type type)
        {
            // Handle IEnumerable<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = type.GetGenericArguments()[0];
                return descriptors.Any(d => d.ServiceType == elementType);
            }
            return descriptors.Any(d => d.ServiceType == type);
        }

        /// <inheritdoc/>
        object IObjectResolver.Resolve(Type type, HashSet<Type> stack) => GetService(type, stack);
    }
}

// TODO: CONSIDER_MULTI_CTOR_SUPPORT
// Currently restricted to a single public constructor (as per Microsoft.Extensions.DependencyInjection).
// Consider relaxing this in future to support constructor scoring or [PreferredConstructor] selection.
