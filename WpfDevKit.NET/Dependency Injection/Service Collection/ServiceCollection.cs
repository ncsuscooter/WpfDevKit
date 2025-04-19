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
        private readonly Dictionary<ServiceDescriptor, List<Delegate>> configurators = new Dictionary<ServiceDescriptor, List<Delegate>>();

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
        /// Thrown if the options service has already been registered.
        /// </exception>
        public IServiceCollection AddOptions<TOptions>(Action<TOptions> configure) where TOptions : class, new() =>
            AddOptions<TOptions>().Configure(configure);

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the options service has already been registered.
        /// </exception>
        public IServiceCollection AddOptions<TOptions>() where TOptions : class, new()
        {
            EnsureNotBuilt();
            EnsureOptionsNotRegistered<TOptions>();
            descriptors.Add(new ServiceDescriptor(typeof(IOptions<TOptions>), typeof(Options<TOptions>), TServiceLifetime.Singleton));
            var descriptor = descriptors.FirstOrDefault(d => d.ServiceType == typeof(IOptions<TOptions>));
            if (!configurators.TryGetValue(descriptor, out var _))
                configurators[descriptor] = new List<Delegate>();
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the options service has already been registered.
        /// </exception>
        public IServiceCollection AddOptions<TOptions>(Func<IServiceProvider, TOptions> factory) where TOptions : class
        {
            EnsureNotBuilt();
            EnsureOptionsNotRegistered<TOptions>();
            descriptors.Add(new ServiceDescriptor(typeof(IOptions<TOptions>), provider => new Options<TOptions>(factory(provider)), TServiceLifetime.Singleton));
            return this;
        }

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
            if (!configurators.TryGetValue(descriptor, out var list))
                throw new InvalidOperationException($"Cannot configure options for '{typeof(TOptions).Name}' because it was not registered using AddOptions<T>().");
            list.Add(configure);
            return this;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to create an instance of <see cref="IOptions{TOptions}"/> services after the service provider has been built.
        /// </exception>
        /// <remarks>
        /// This method will emit a warning to the console if the same <c>ServiceType</c> is registered multiple times.
        /// </remarks>
        public IServiceProvider Build()
        {
            EnsureNotBuilt();
            IsBuilt = true;
            var dups = new HashSet<Type>();
            var warn = new HashSet<Type>();
            var copy = new List<ServiceDescriptor>();
            foreach (var descriptor in descriptors)
            {
                if (!dups.Add(descriptor.ServiceType) && warn.Add(descriptor.ServiceType))
                    Debug.WriteLine($"[DI WARNING] ServiceType '{descriptor.ServiceType.FullName}' registered multiple times.");
                if (descriptor.ServiceType.IsGenericType && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(IOptions<>) && descriptor.Factory == null)
                {
                    var optionsType = descriptor.ServiceType.GetGenericArguments()[0];
                    if (optionsType.GetConstructor(Type.EmptyTypes) == null)
                        throw new InvalidOperationException($"Options type '{optionsType.Name}' must have a public parameterless constructor.");
                    var optionsInstance = Activator.CreateInstance(optionsType);
                    foreach (var config in configurators[descriptor])
                        config.DynamicInvoke(optionsInstance);
                    var wrapped = Activator.CreateInstance(typeof(Options<>).MakeGenericType(optionsType), optionsInstance);
                    copy.Add(new ServiceDescriptor(descriptor.ServiceType, _ => wrapped, TServiceLifetime.Singleton));
                    Debug.WriteLine($"[DI] Options ServiceType '{descriptor.ServiceType.FullName}' built.");
                }
                else
                {
                    copy.Add(descriptor);
                    Debug.WriteLine($"[DI] ServiceType '{descriptor.ServiceType.FullName}' built.");
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
        /// Ensures that the service collection has not been built.
        /// Throws an exception if service registration is attempted after building the provider.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register services after the service provider has been built.
        /// </exception>
        private void EnsureOptionsNotRegistered<TOptions>() where TOptions : class
        {
            if (descriptors.Any(d => d.ServiceType == typeof(IOptions<TOptions>)))
                throw new InvalidOperationException($"Options for '{typeof(TOptions).Name}' have already been registered.");
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

// TODO: SUPPORT_OPEN_GENERIC_TYPES
// Open generics like IRepository<T> → Repository<T> are not supported yet.
// Consider extending descriptor matching to handle generic type definitions and close them at resolution time.
