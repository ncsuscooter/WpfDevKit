using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WpfDevKit.Interfaces;

namespace WpfDevKit.Mvvm
{
    internal class ResolvableFactory : IResolvableFactory
    {
        private readonly IServiceProvider serviceProvider;
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> constructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> injectablePropertiesCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        public ResolvableFactory(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;
        public TClass Create<TClass>(params object[] parameters) where TClass : class => (TClass)Create(typeof(TClass), parameters);
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

            var result = constructor.Invoke(args);
            var injectableProps = injectablePropertiesCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                                                 .Where(p => p.IsDefined(typeof(ResolvableAttribute), inherit: true) && p.CanWrite)
                                                                                 .ToList());

            foreach (var prop in injectableProps)
            {
                var service = serviceProvider.GetService(prop.PropertyType);
                if (service != null)
                {
                    prop.SetValue(result, service);
                }
                else
                {
                    // Optional enhancement: log or throw if service is missing
                    System.Diagnostics.Debug.WriteLine($"[Inject] Warning: No service found for property {prop.Name} ({prop.PropertyType.Name}) on {type.Name}.");
                }
            }

            return result;
        }
    }
}
