namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Specifies the lifetime of a service in the dependency injection container.
    /// </summary>
    internal enum TServiceLifetime
    {
        /// <summary>
        /// A single instance is created and shared for the lifetime of the application.
        /// </summary>
        Singleton,

        /// <summary>
        /// A new instance is created every time the service is requested.
        /// </summary>
        Transient
    }
}
