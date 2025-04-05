using System;

namespace WpfDevKit.Interfaces
{
    /// <summary>
    /// Defines a contract for creating class instances with optional dependency resolution
    /// and runtime-provided parameters.
    /// </summary>
    public interface IResolvableFactory
    {
        /// <summary>
        /// Creates an instance of the specified class type, resolving constructor dependencies
        /// via the internal service provider and applying any optional parameters.
        /// </summary>
        /// <typeparam name="TClass">The type of class to instantiate. Must be a reference type.</typeparam>
        /// <param name="parameters">
        /// Optional parameters to be matched against the constructor signature. These take precedence
        /// over resolved services when matching argument types.
        /// </param>
        /// <returns>
        /// A fully constructed instance of <typeparamref name="TClass"/> with dependencies injected.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a suitable constructor could not be found or required dependencies could not be resolved.
        /// </exception>
        TClass Create<TClass>(params object[] parameters) where TClass : class;

        /// <summary>
        /// Creates an instance of the specified class type, resolving constructor dependencies
        /// via the internal service provider and applying any optional parameters.
        /// </summary>
        /// <param name="type">the type of the object to be created.</param>
        /// <param name="parameters">
        /// Optional parameters to be matched against the constructor signature. These take precedence
        /// over resolved services when matching argument types.
        /// </param>
        /// <returns>
        /// A fully constructed instance of <paramref name="type"/> with dependencies injected.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if type is not a class or a suitable constructor could not be found or required dependencies could not be resolved.
        /// </exception>
        object Create(Type type, params object[] parameters);
    }
}
