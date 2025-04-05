using System;
using System.Collections.Generic;
using System.Linq;
using WpfDevKit.DependencyInjection.Interfaces;

namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Provides a collection for registering service descriptors and building a service provider.
    /// </summary>
    internal class ServiceCollection : IServiceCollection
    {
        private readonly List<ServiceDescriptor> descriptors = new List<ServiceDescriptor>();

        /// <summary>
        /// Gets a value indicating whether the service provider has been built.
        /// Once built, no additional services can be registered.
        /// </summary>
        public bool IsBuilt { get; private set; }

        /// <summary>
        /// Registers a singleton service with a default constructor.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddSingleton<TService, TImplementation>() where TImplementation : TService, new()
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), TServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Registers a singleton service with a default constructor.
        /// </summary>
        /// <param name="serviceType">The service type.</typeparam>
        /// <param name="implementationType">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddSingleton(Type serviceType, Type implementationType)
        {
            EnsureNotBuilt();
            EnsureTypeCriteria(serviceType, implementationType);
            descriptors.Add(new ServiceDescriptor(serviceType, implementationType, TServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Registers a singleton service using a factory method.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory method used to create the service instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddSingleton<TService>(Func<IServiceProvider, object> factory)
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), factory, TServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Registers a transient service with a default constructor.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddTransient<TService, TImplementation>() where TImplementation : TService, new()
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), TServiceLifetime.Transient));
            return this;
        }

        /// <summary>
        /// Registers a transient service with a default constructor.
        /// </summary>
        /// <param name="serviceType">The service type.</typeparam>
        /// <param name="implementationType">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddTransient(Type serviceType, Type implementationType)
        {
            EnsureNotBuilt();
            EnsureTypeCriteria(serviceType, implementationType);
            descriptors.Add(new ServiceDescriptor(serviceType, implementationType, TServiceLifetime.Transient));
            return this;
        }

        /// <summary>
        /// Registers a transient service using a factory method.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory method used to create the service instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddTransient<TService>(Func<IServiceProvider, object> factory)
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), factory, TServiceLifetime.Transient));
            return this;
        }

        /// <summary>
        /// Registers configuration options for the specified type.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddOptions<T>() where T : class, new() => AddOptions<T>(configure: null);

        /// <summary>
        /// Registers configuration options for the specified type using a configuration action.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <param name="configure">The configuration action to apply to the options instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddOptions<T>(Action<T> configure) where T : class, new()
        {
            EnsureNotBuilt();
            var options = new T();
            configure?.Invoke(options);
            descriptors.Add(new ServiceDescriptor(typeof(IOptions<T>), provider => new Options<T>(options), TServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Registers configuration options for the specified type using a factory method.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <param name="factory">The factory method used to create the options instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddOptions<T>(Func<IServiceProvider, T> factory) where T : class, new()
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(IOptions<T>), provider => new Options<T>(factory(provider)), TServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Builds the service provider from the registered descriptors.
        /// </summary>
        /// <returns>A configured <see cref="IServiceProvider"/> instance.</returns>
        public IServiceProvider Build()
        {
            IsBuilt = true;
            var copy = descriptors.ToList();
            descriptors.Clear();
            return new ServiceProvider(copy);
        }

        /// <summary>
        /// Ensures that the service collection has not been built.
        /// Throws an exception if service registration is attempted after building the provider.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        private void EnsureNotBuilt()
        {
            if (IsBuilt)
                throw new InvalidOperationException("Cannot register new services after the service provider has been built.");
        }

        /// <summary>
        /// Validates that the implementation type is assignable to the service type and has a public parameterless constructor.
        /// </summary>
        /// <param name="serviceType">The type of the service interface or base class.</param>
        /// <param name="implementationType">The type implementing or deriving from the service type.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="serviceType"/> or <paramref name="implementationType"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="implementationType"/> does not implement or inherit from <paramref name="serviceType"/> or does not have a public parameterless constructor.
        /// </exception>
        private void EnsureTypeCriteria(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (!serviceType.IsAssignableFrom(implementationType))
                throw new InvalidOperationException($"{implementationType.Name} must implement or inherit from {serviceType.Name}");
            if (implementationType.GetConstructors().All(ctor => ctor.GetParameters().Length > 0))
                throw new InvalidOperationException($"{implementationType.Name} doesn't have a public parameterless constructor");
        }
    }
}
