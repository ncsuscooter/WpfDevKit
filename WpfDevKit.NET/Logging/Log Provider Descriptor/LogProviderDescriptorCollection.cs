using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides a registry for mapping <see cref="ILogProvider"/> types to their corresponding <see cref="LogProviderDescriptor"/>.
    /// This registry allows fast, type-safe retrieval of descriptors at runtime without requiring enumeration or reflection.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogProviderDescriptorCollection
    {
        private readonly Dictionary<Type, LogProviderDescriptor> descriptors =
            new Dictionary<Type, LogProviderDescriptor>();

        /// <summary>
        /// Registers a descriptor for a given <typeparamref name="TProvider"/> type.
        /// </summary>
        /// <typeparam name="TProvider">The type of the log provider.</typeparam>
        /// <param name="descriptor">The descriptor to associate with the provider type.</param>
        public void Add<TProvider>(LogProviderDescriptor descriptor) where TProvider : ILogProvider =>
            descriptors[typeof(TProvider)] = descriptor;

        /// <summary>
        /// Retrieves all registered <see cref="LogProviderDescriptor"/> instances in the registry.
        /// </summary>
        /// <returns>
        /// An <see cref="IReadOnlyCollection{T}"/> containing all descriptors currently registered.
        /// </returns>
        public IReadOnlyCollection<LogProviderDescriptor> GetDescriptors() => descriptors.Values;

        /// <summary>
        /// Retrieves the descriptor associated with the specified provider type.
        /// </summary>
        /// <param name="type">The type of the provider for which to retrieve the descriptor.</param>
        /// <returns>The corresponding <see cref="LogProviderDescriptor"/> instance, or <c>null</c> if not found.</returns>
        public LogProviderDescriptor GetDescriptor(Type type) =>
            descriptors.TryGetValue(type, out var descriptor) ? descriptor : null;

        /// <summary>
        /// Retrieves the descriptor associated with the specified provider type.
        /// </summary>
        /// <typeparam name="TProvider">The generic type of the log provider.</typeparam>
        /// <returns>The corresponding <see cref="LogProviderDescriptor"/> instance, or <c>null</c> if not found.</returns>
        public LogProviderDescriptor GetDescriptor<TProvider>() where TProvider : ILogProvider =>
            GetDescriptor(typeof(TProvider));
    }
}
