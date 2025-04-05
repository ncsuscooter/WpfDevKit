using System;
using System.Collections.Generic;
using System.Linq;
using WpfDevKit.Mvvm;

namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Provides service resolution for registered services and options.
    /// </summary>
    internal class ServiceProvider : IServiceProvider
    {
        private readonly List<ServiceDescriptor> descriptors;
        private readonly ResolvableFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
        /// </summary>
        /// <param name="descriptors">The collection of registered service descriptors.</param>
        public ServiceProvider(List<ServiceDescriptor> descriptors) => (this.descriptors, factory) = (descriptors, new ResolvableFactory(this));

        /// <summary>
        /// Gets a service object of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve.</param>
        /// <returns>The resolved service instance, or <c>null</c> if not found.</returns>
        /// <exception cref="Exception">Thrown when the specified service type is not registered.</exception>
        public object GetService(Type serviceType)
        {
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                var matches = descriptors.Where(d => d.ServiceType == elementType).ToList();
                var instances = new List<object>();

                foreach (var descriptor in matches)
                {
                    if (descriptor.Lifetime == TServiceLifetime.Singleton)
                    {
                        if (descriptor.Instance == null)
                            descriptor.Instance = CreateInstance(descriptor);
                        instances.Add(descriptor.Instance);
                    }
                    else
                    {
                        instances.Add(CreateInstance(descriptor));
                    }
                }

                var array = Array.CreateInstance(elementType, instances.Count);
                instances.ToArray().CopyTo(array, 0);
                return array;
            }

            var singleDescriptor = descriptors.FirstOrDefault(x => x.ServiceType == serviceType) ?? 
                throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");

            if (singleDescriptor.Lifetime == TServiceLifetime.Singleton)
            {
                if (singleDescriptor.Instance == null)
                    singleDescriptor.Instance = CreateInstance(singleDescriptor);
                return singleDescriptor.Instance;
            }

            return CreateInstance(singleDescriptor);
        }

        /// <summary>
        /// Creates an instance of the specified service descriptor.
        /// </summary>
        /// <param name="descriptor">The service descriptor to create an instance from.</param>
        /// <returns>The created service instance.</returns>
        private object CreateInstance(ServiceDescriptor descriptor)
        {
            if (descriptor.Factory != null)
                return descriptor.Factory(this);
            return factory.Create(descriptor.ImplementationType);
        }
    }
}
