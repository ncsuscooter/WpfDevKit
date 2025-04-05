using System;

namespace WpfDevKit.DependencyInjection.Interfaces
{
    /// <summary>
    /// Defines methods for registering services and options, and for building a service provider.
    /// </summary>
    public interface IServiceCollection
    {
        /// <summary>
        /// Gets a value indicating whether the service provider has been built.
        /// Once built, no additional services can be registered.
        /// </summary>
        bool IsBuilt { get; }

        /// <summary>
        /// Registers a singleton service of the specified type with an implementation type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddSingleton<TService, TImplementation>() where TImplementation : TService, new();

        /// <summary>
        /// Registers a singleton service of the specified type with an implementation type.
        /// </summary>
        /// <param name="serviceType">The service type.</typeparam>
        /// <param name="implementationType">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddSingleton(Type serviceType, Type implementationType);

        /// <summary>
        /// Registers a singleton service of the specified type using a factory method.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory method to create the service instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddSingleton<TService>(Func<IServiceProvider, object> factory);

        /// <summary>
        /// Registers a transient service of the specified type with an implementation type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddTransient<TService, TImplementation>() where TImplementation : TService, new();

        /// <summary>
        /// Registers a transient service of the specified type with an implementation type.
        /// </summary>
        /// <param name="serviceType">The service type.</typeparam>
        /// <param name="implementationType">The implementation type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddTransient(Type serviceType, Type implementationType);

        /// <summary>
        /// Registers a transient service of the specified type using a factory method.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The factory method to create the service instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddTransient<TService>(Func<IServiceProvider, object> factory);

        /// <summary>
        /// Registers an options instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddOptions<T>() where T : class, new();

        /// <summary>
        /// Registers an options instance of the specified type with a configuration delegate.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <param name="configure">The delegate used to configure the options instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddOptions<T>(Action<T> configure) where T : class, new();

        /// <summary>
        /// Registers an options instance of the specified type using a factory method.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <param name="factory">The factory method to create the options instance.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance.</returns>
        IServiceCollection AddOptions<T>(Func<IServiceProvider, T> factory) where T : class, new();

        /// <summary>
        /// Builds the service provider.
        /// </summary>
        /// <returns>The built <see cref="IServiceProvider"/> instance.</returns>
        IServiceProvider Build();
    }
}
