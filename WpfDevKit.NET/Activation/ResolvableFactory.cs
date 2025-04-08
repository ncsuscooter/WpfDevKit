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
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvableFactory"/> class
        /// using the specified service provider.
        /// </summary>
        /// <param name="provider">The service provider used to resolve dependencies.</param>
        public ResolvableFactory(IServiceProvider provider) => this.provider = provider;

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
        /// <param name="args">Optional parameters used to assist with constructor resolution.</param>
        /// <returns>A new instance of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided <paramref name="type"/> is not a class.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no public constructor is found or dependencies cannot be resolved.</exception>
        public object Create(Type type, params object[] args)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!type.IsClass) throw new ArgumentException("Type must be a class", nameof(type));

            var constructor = constructorCache.GetOrAdd(type, ResolveConstructor);
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                var match = args?.FirstOrDefault(a => paramType.IsInstanceOfType(a));

                if (match != null)
                {
                    arguments[i] = match;
                }
                else
                {
                    arguments[i] = provider.GetRequiredService(paramType);
                }
            }

            var result = constructor.Invoke(arguments);
            var properties = propertiesCache.GetOrAdd(type, ResolveProperties);

            foreach (var prop in properties)
            {
                var service = provider.GetService(prop.PropertyType);
                if (service != null)
                {
                    prop.SetValue(result, service);
                }
                else
                {
                    provider.GetService<InternalLogger>()?.LogMessage?.Invoke($"[Inject] Warning: No service found for property",
                                                                              $"Property Name: {prop.Name} - Property Type: {prop.PropertyType.Name} Service: {type.Name}.",
                                                                              default);
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the best-fit constructor for a type, preferring the one with the most resolvable parameters.
        /// </summary>
        /// <param name="type">The type for which to resolve a constructor.</param>
        /// <returns>The selected constructor.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no suitable constructor is found.</exception>
        private ConstructorInfo ResolveConstructor(Type type) => type
            .GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length == 0 || parameters.All(p => provider.GetService(p.ParameterType) != null);
            })
            ?? throw new InvalidOperationException($"No suitable public constructor found for {type.Name}.");

        /// <summary>
        /// Resolves the public setters for an instance type marked with the <see cref="ResolvableAttribute"/> for dependency injection via properties.
        /// </summary>
        /// <param name="type">The type for which to resolve properties.</param>
        /// <returns>The collection of properties.</returns>
        private List<PropertyInfo> ResolveProperties(Type type) => type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.IsDefined(typeof(ResolvableAttribute), inherit: true) && p.CanWrite)
            .ToList();
    }
}
