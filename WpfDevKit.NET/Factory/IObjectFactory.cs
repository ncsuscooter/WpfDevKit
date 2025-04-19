using System;

namespace WpfDevKit.Factory
{
    /// <summary>
    /// Defines a contract for creating object instances with optional runtime parameters
    /// and dependency injection through an internal service provider.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates an instance of <typeparamref name="T"/>, resolving constructor parameters
        /// using the registered services and applying any provided arguments.
        /// </summary>
        /// <typeparam name="T">The type of object to create. Must be a reference type.</typeparam>
        /// <param name="args">Optional arguments to assist in constructor selection and injection.</param>
        /// <returns>A fully constructed instance of <typeparamref name="T"/> with dependencies resolved.</returns>
        T Create<T>(params object[] args) where T : class;

        /// <summary>
        /// Creates an instance of the specified <paramref name="type"/>, resolving constructor parameters
        /// using the registered services and applying any provided arguments.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="args">Optional arguments to assist in constructor selection and injection.</param>
        /// <returns>A fully constructed instance of the specified <paramref name="type"/>.</returns>
        object Create(Type type, params object[] args);
    }
}
