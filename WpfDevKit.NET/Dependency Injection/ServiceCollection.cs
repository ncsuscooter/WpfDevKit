using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Provides a collection for registering service descriptors and building a service provider.
    /// </summary>
    [DebuggerStepThrough]
    internal class ServiceCollection : IServiceCollection
    {
        private readonly List<ServiceDescriptor> descriptors = new List<ServiceDescriptor>();

        /// <inheritdoc/>
        public bool IsBuilt { get; private set; }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        public IServiceCollection AddSingleton<TImplementation>() where TImplementation : class =>
            AddSingleton(typeof(TImplementation), typeof(TImplementation));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        public IServiceCollection AddSingleton<TService, TImplementation>() where TImplementation : class, TService => 
            AddSingleton(typeof(TService), typeof(TImplementation));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="serviceType"/> or <paramref name="implementationType"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="implementationType"/> does not implement or inherit from <paramref name="serviceType"/>.
        /// </exception>
        public IServiceCollection AddSingleton(Type serviceType, Type implementationType)
        {
            EnsureNotBuilt();
            EnsureTypeCriteria(serviceType, implementationType);
            descriptors.Add(new ServiceDescriptor(serviceType, implementationType, TServiceLifetime.Singleton));
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        public IServiceCollection AddSingleton<TService>(Func<IServiceProvider, object> factory)
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), factory, TServiceLifetime.Singleton));
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        public IServiceCollection AddTransient<TImplementation>() where TImplementation : class =>
            AddTransient(typeof(TImplementation), typeof(TImplementation));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        public IServiceCollection AddTransient<TService, TImplementation>() where TImplementation : class, TService => 
            AddTransient(typeof(TService), typeof(TImplementation));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="serviceType"/> or <paramref name="implementationType"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="implementationType"/> does not implement or inherit from <paramref name="serviceType"/>.
        /// </exception>
        public IServiceCollection AddTransient(Type serviceType, Type implementationType)
        {
            EnsureNotBuilt();
            EnsureTypeCriteria(serviceType, implementationType);
            descriptors.Add(new ServiceDescriptor(serviceType, implementationType, TServiceLifetime.Transient));
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        public IServiceCollection AddTransient<TService>(Func<IServiceProvider, object> factory)
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), factory, TServiceLifetime.Transient));
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to add options after the service provider has been built.
        /// </exception>
        public IServiceCollection AddOptions<TOptions>() where TOptions : class, new() => 
            AddSingleton<IOptions<TOptions>>();

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to add options after the service provider has been built.
        /// </exception>
        public IServiceCollection AddOptions<TOptions>(Action<TOptions> configure) where TOptions : class, new() => 
            AddSingleton<IOptions<TOptions>>().Configure(configure);

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to add options after the service provider has been built.
        /// </exception>
        public IServiceCollection AddOptions<TOptions>(Func<IServiceProvider, TOptions> factory) where TOptions : class, new() => 
            AddSingleton<IOptions<TOptions>>(provider => new Options<TOptions>(factory(provider)));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the options were registered using a factory method.
        /// </exception>
        public IServiceCollection Configure<TOptions>(Action<TOptions> configure) where TOptions : class
        {
            EnsureNotBuilt();
            var collection = descriptors.Where(d => d.ServiceType == typeof(IOptions<TOptions>)).ToList();
            if (collection == null || collection.Count == 0)
                throw new InvalidOperationException($"Cannot configure options for '{typeof(TOptions).Name}' because it is not registered as a service.");
            var descriptor = collection.FirstOrDefault(d => d.Factory == null) ??
                throw new InvalidOperationException($"Cannot configure options for '{typeof(TOptions).Name}' because it is registered using a factory.");
            descriptor.OptionConfigurators.Add(instance => configure((TOptions)instance));
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to create an instance of <see cref="IOptions{TOptions}"/> services after the service provider has been built.
        /// </exception>
        public IServiceProvider Build()
        {
            EnsureNotBuilt();
            IsBuilt = true;
            var copy = new List<ServiceDescriptor>();
            foreach (var item in descriptors)
            {
                if (item.ServiceType.IsGenericType && item.ServiceType.GetGenericTypeDefinition() == typeof(IOptions<>) && item.Factory == null)
                {
                    var optionsType = item.ServiceType.GetGenericArguments()[0];
                    var optionsInstance = Activator.CreateInstance(optionsType);
                    foreach (var config in item.OptionConfigurators)
                        config.DynamicInvoke(optionsInstance);
                    item.OptionConfigurators.Clear();
                    var wrapped = Activator.CreateInstance(typeof(Options<>).MakeGenericType(optionsType), optionsInstance);
                    copy.Add(new ServiceDescriptor(item.ServiceType, _ => wrapped, TServiceLifetime.Singleton));
                }
                else
                {
                    copy.Add(item);
                }
            }
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
                throw new InvalidOperationException("Cannot register new services or build a new service provider after the service provider has been built.");
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
        /// Thrown if <paramref name="implementationType"/> does not implement or inherit from <paramref name="serviceType"/>.
        /// </exception>
        private void EnsureTypeCriteria(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (!serviceType.IsAssignableFrom(implementationType))
                throw new InvalidOperationException($"{implementationType.Name} must implement or inherit from {serviceType.Name}");
        }
    }
}
