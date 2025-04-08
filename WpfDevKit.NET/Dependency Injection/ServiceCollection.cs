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
        public IServiceCollection AddSingleton<TImplementation>() where TImplementation : class =>
            AddSingleton(typeof(TImplementation), typeof(TImplementation));

        /// <inheritdoc/>
        public IServiceCollection AddSingleton<TService, TImplementation>() where TImplementation : class, TService => 
            AddSingleton(typeof(TService), typeof(TImplementation));

        /// <inheritdoc/>
        public IServiceCollection AddSingleton(Type serviceType, Type implementationType)
        {
            EnsureNotBuilt();
            EnsureTypeCriteria(serviceType, implementationType);
            descriptors.Add(new ServiceDescriptor(serviceType, implementationType, TServiceLifetime.Singleton));
            return this;
        }

        /// <inheritdoc/>
        public IServiceCollection AddSingleton<TService>(Func<IServiceProvider, object> factory)
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), factory, TServiceLifetime.Singleton));
            return this;
        }

        /// <inheritdoc/>
        public IServiceCollection AddTransient<TImplementation>() where TImplementation : class =>
            AddSingleton(typeof(TImplementation), typeof(TImplementation));

        /// <inheritdoc/>
        public IServiceCollection AddTransient<TService, TImplementation>() where TImplementation : class, TService => 
            AddTransient(typeof(TService), typeof(TImplementation));

        /// <inheritdoc/>
        public IServiceCollection AddTransient(Type serviceType, Type implementationType)
        {
            EnsureNotBuilt();
            EnsureTypeCriteria(serviceType, implementationType);
            descriptors.Add(new ServiceDescriptor(serviceType, implementationType, TServiceLifetime.Transient));
            return this;
        }

        /// <inheritdoc/>
        public IServiceCollection AddTransient<TService>(Func<IServiceProvider, object> factory)
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(TService), factory, TServiceLifetime.Transient));
            return this;
        }

        /// <inheritdoc/>
        public IServiceCollection AddOptions<TOptions>() where TOptions : class, new() => AddOptions<TOptions>(configure: null);

        /// <inheritdoc/>
        public IServiceCollection AddOptions<TOptions>(Action<TOptions> configure) where TOptions : class, new()
        {
            EnsureNotBuilt();
            var options = new TOptions();
            configure?.Invoke(options);
            descriptors.Add(new ServiceDescriptor(typeof(IOptions<TOptions>), provider => new Options<TOptions>(options), TServiceLifetime.Singleton));
            return this;
        }

        /// <inheritdoc/>
        public IServiceCollection AddOptions<TOptions>(Func<IServiceProvider, TOptions> factory) where TOptions : class, new()
        {
            EnsureNotBuilt();
            descriptors.Add(new ServiceDescriptor(typeof(IOptions<TOptions>), provider => new Options<TOptions>(factory(provider)), TServiceLifetime.Singleton));
            return this;
        }

        /// <inheritdoc/>
        public IServiceProvider Build()
        {
            EnsureNotBuilt();
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
