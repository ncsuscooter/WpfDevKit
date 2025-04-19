namespace WpfDevKit.Logging
{
    public interface ILogProviderOptions
    {
        /// <summary>
        /// Gets or sets the categories of log messages that are enabled for logging.
        /// </summary>
        /// <remarks>
        TLogCategory EnabledCategories { get; set; }

        /// <summary>
        /// Gets the categories of log messages that are disabled from logging.
        /// </summary>
        TLogCategory DisabledCategories { get; }
    }
}
