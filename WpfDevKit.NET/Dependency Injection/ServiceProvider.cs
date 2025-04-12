using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using WpfDevKit.Activation;

namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Provides resolution of services registered in a <see cref="ServiceCollection"/>.
    /// 
    /// Supports constructor injection, property injection using the <see cref="ResolvableAttribute"/>,
    /// circular dependency detection, and enumeration of multiple registered services via <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal class ServiceProvider : IServiceProvider
    {
        private readonly ConcurrentDictionary<Type, ConstructorInfo> constructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();
        private readonly ConcurrentDictionary<Type, List<PropertyInfo>> propertiesCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        private readonly List<ServiceDescriptor> descriptors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class with the provided service descriptors.
        /// </summary>
        /// <param name="descriptors">The collection of registered services.</param>
        public ServiceProvider(List<ServiceDescriptor> descriptors) => this.descriptors = descriptors;

        /// <summary>
        /// Resolves the service of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve.</typeparam>
        /// <remarks>
        /// If multiple services of the same type are registered, 
        /// this method will always return the last registered service.
        /// </remarks>
        /// <returns>The resolved instance or null if not registered.</returns>
        public TService GetService<TService>() => (TService)GetService(typeof(TService));

        /// <summary>
        /// Resolves the specified service type.
        /// </summary>
        /// <param name="serviceType">The type of the service to resolve.</param>
        /// <remarks>
        /// If multiple services of the same type are registered, 
        /// this method will always return the last registered service.
        /// </remarks>
        /// <returns>An instance of the requested service or null if not registered.</returns>
        public object GetService(Type serviceType) => GetService(serviceType, new HashSet<Type>());

        /// <summary>
        /// Resolves the specified service type, optionally detecting circular dependencies.
        /// </summary>
        /// <param name="serviceType">The type of service to resolve.</param>
        /// <param name="stack">A hash set used to track recursion and detect circular dependencies.</param>
        /// <returns>The resolved service instance.</returns>
        private object GetService(Type serviceType, HashSet<Type> stack)
        {
            // Support IEnumerable<T>
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                var matches = descriptors.Where(d => d.ServiceType == elementType).ToList();
                var instances = new List<object>();

                foreach (var d in matches)
                    instances.Add(GetService(d.ServiceType, stack));

                var array = Array.CreateInstance(elementType, instances.Count);
                instances.ToArray().CopyTo(array, 0);
                return array;
            }

            // Get the last registered service (consistent with Microsoft.Extensions.DependencyInjection)
            var descriptor = descriptors.LastOrDefault(x => x.ServiceType == serviceType);
            if (descriptor == null)
                return default;

            if (descriptor.Lifetime == TServiceLifetime.Singleton)
            {
                if (descriptor.Instance == null)
                    descriptor.Instance = GetService(descriptor, stack);
                return descriptor.Instance;
            }

            return GetService(descriptor, stack);
        }

        /// <summary>
        /// Instantiates the service using either a factory or constructor + DI + property injection.
        /// </summary>
        /// <param name="descriptor">The service descriptor.</param>
        /// <param name="stack">Tracking stack for circular dependencies.</param>
        /// <returns>The created service instance.</returns>
        private object GetService(ServiceDescriptor descriptor, HashSet<Type> stack)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            // Prefer factory delegate if defined
            if (descriptor.Factory != null)
                return descriptor.Factory(this);

            var type = descriptor.ImplementationType;
            if (!type.IsClass)
                throw new ArgumentException("Implementation type must be a class.", nameof(type));

            if (!stack.Add(type))
                throw new InvalidOperationException($"Circular dependency detected for type {type.Name}.");

            try
            {
                // Use cached constructor or find exactly one public constructor
                var constructor = constructorCache.GetOrAdd(type, t =>
                {
                    var ctors = t.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                    if (ctors.Length != 1)
                        throw new InvalidOperationException($"Type '{t.Name}' must have exactly one public constructor. Found: {ctors.Length}.");
                    return ctors[0];
                });

                // Resolve constructor arguments
                var parameters = constructor.GetParameters();
                var arguments = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var service = GetService(param.ParameterType, stack);
                    if (service != null)
                        arguments[i] = service;
                    else if (param.HasDefaultValue)
                        arguments[i] = param.DefaultValue;
                    else
                        throw new InvalidOperationException($"Cannot resolve parameter '{param.Name}' of type '{param.ParameterType.Name}'.");
                }

                var instance = constructor.Invoke(arguments);

                // Inject properties marked with [Resolvable]
                var properties = propertiesCache.GetOrAdd(type, t =>
                    t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(p => p.IsDefined(typeof(ResolvableAttribute), inherit: true) && p.CanWrite)
                     .ToList());

                foreach (var prop in properties)
                {
                    var service = GetService(prop.PropertyType, stack);
                    if (service != null)
                        prop.SetValue(instance, service);
                    else
                        Debug.WriteLine($"[Inject] Warning: No service ({type.Name}) found for property ({prop.Name}).");
                }

                return instance;
            }
            finally
            {
                stack.Remove(type);
            }
        }
    }
}
