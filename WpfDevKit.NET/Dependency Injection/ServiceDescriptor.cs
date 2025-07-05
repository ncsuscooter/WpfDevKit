using System;
using System.Diagnostics;

namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Describes a service, its implementation, and its lifetime.
    /// </summary>
    [DebuggerStepThrough]
    internal class ServiceDescriptor
    {
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the implementation type of the service.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets the lifetime of the service.
        /// </summary>
        public TServiceLifetime Lifetime { get; }

        /// <summary>
        /// Gets the factory used to create the service instance.
        /// </summary>
        public Func<IServiceProvider, object> Factory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDescriptor"/> class with an implementation type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationType">The type implementing the service.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        public ServiceDescriptor(Type serviceType, Type implementationType, TServiceLifetime lifetime)
        {
            ServiceType  = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDescriptor"/> class with a factory method.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="factory">The factory method used to create the service instance.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, TServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            Factory = factory;
            Lifetime= lifetime;
        }
    }
}
