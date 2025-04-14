namespace WpfDevKit.DependencyInjection
{
    /// <summary>
    /// Provides access to a configured instance of <typeparamref name="TOptions"/>.
    /// Typically used for accessing application or service configuration options.
    /// </summary>
    /// <typeparam name="TOptions">The type of options.</typeparam>
    public interface IOptions<TOptions> where TOptions : class
    {
        /// <summary>
        /// Gets the configured options instance.
        /// </summary>
        TOptions Value { get; }
    }
}
