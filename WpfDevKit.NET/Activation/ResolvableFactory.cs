using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WpfDevKit.Activation
{
    /// <summary>
    /// Default implementation of <see cref="IResolvableFactory"/> that resolves
    /// and creates instances of classes using constructor and property injection.
    /// Supports dynamic parameters and attributes for fine-grained control.
    /// </summary>
    internal class ResolvableFactory : IResolvableFactory
    {
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> constructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> propertiesCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvableFactory"/> class
        /// using the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        public ResolvableFactory(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

        /// <summary>
        /// Creates an instance of the specified class type, resolving dependencies
        /// via constructor and property injection.
        /// </summary>
        /// <typeparam name="TClass">The type of class to instantiate.</typeparam>
        /// <param name="parameters">Optional parameters to assist with constructor resolution.</param>
        /// <returns>A fully constructed instance of <typeparamref name="TClass"/>.</returns>
        public TClass Create<TClass>(params object[] parameters) where TClass : class => (TClass)Create(typeof(TClass), parameters);

        /// <summary>
        /// Creates an instance of the specified type using the most suitable constructor
        /// and resolves any [Resolvable]-decorated properties after instantiation.
        /// </summary>
        /// <param name="type">The type to create. Must be a non-abstract class.</param>
        /// <param name="parameters">Optional parameters used to assist with constructor resolution.</param>
        /// <returns>A new instance of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided <paramref name="type"/> is not a class.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no public constructor is found or dependencies cannot be resolved.</exception>
        public object Create(Type type, params object[] parameters)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsClass)
                throw new ArgumentException("Type must be a class", nameof(type));
            var constructor = constructorCache.GetOrAdd(type, t => t.GetConstructors()
                                                                    .OrderByDescending(c => c.GetParameters().Length)
                                                                    .FirstOrDefault() ?? throw new InvalidOperationException($"No public constructor found for {t.Name}."));

            var ctorParams = constructor.GetParameters();
            var args = new object[ctorParams.Length];
            var dynamicArgs = parameters?.ToList() ?? new List<object>();

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var paramType = ctorParams[i].ParameterType;
                var service = serviceProvider.GetService(paramType);
                if (service != null)
                {
                    args[i] = service;
                    continue;
                }
                var match = dynamicArgs.FirstOrDefault(p => paramType.IsInstanceOfType(p));
                if (match != null)
                {
                    args[i] = match;
                    dynamicArgs.Remove(match);
                    continue;
                }
                throw new InvalidOperationException($"Unable to resolve constructor parameter '{paramType.Name}' for type '{type.Name}'.");
            }

            var logger = serviceProvider.GetService<InternalLogger>();
            var result = constructor.Invoke(args);
            var props = propertiesCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                             .Where(p => p.IsDefined(typeof(ResolvableAttribute), inherit: true) && p.CanWrite)
                                                             .ToList());

            foreach (var prop in props)
            {
                var service = serviceProvider.GetService(prop.PropertyType);
                if (service != null)
                {
                    prop.SetValue(result, service);
                }
                else
                {
                    logger?.LogMessage?.Invoke($"[Inject] Warning: No service found for property", $"Property Name: {prop.Name} - Property Type: {prop.PropertyType.Name} Service: {type.Name}.", default);
                }
            }

            return result;
        }
    }
}
