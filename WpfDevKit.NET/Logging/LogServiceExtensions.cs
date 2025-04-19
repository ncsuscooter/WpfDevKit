using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides extension methods for the <see cref="ILogService"/> interface to facilitate logging at different levels and task lifecycle management.
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    [DebuggerStepThrough]
    public static class LogServiceExtensions
    {
        /// <summary>
        /// Registers logging services.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddLoggingService(this IServiceCollection services) =>
            services.AddSingleton<LogMetrics>()
                    .AddSingleton<LogQueue>()
                    .AddSingleton<LogService>()
                    .AddSingleton<LogBackgroundService>()
                    .AddSingleton<LogProviderDescriptorCollection>()
                    .AddLogProvider<MemoryLogProvider, MemoryLogProviderOptions>()
                    .AddLogProvider<ConsoleLogProvider, ConsoleLogProviderOptions>()
                    .AddLogProvider<DatabaseLogProvider, DatabaseLogProviderOptions>()
                    .AddLogProvider<UserLogProvider, UserLogProviderOptions>()
                    .AddSingleton<IGetLogs>(p => p.GetRequiredService<UserLogProvider>())
                    .AddSingleton<IGetLogs>(p => p.GetRequiredService<MemoryLogProvider>())
                    .AddSingleton<ILogService>(p => p.GetRequiredService<LogService>())
                    .AddSingleton<ILogMetricsReader>(p => p.GetRequiredService<LogMetrics>());

        /// <summary>
        /// Registers a logging provider and its associated options, descriptor, and implementation into the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TProvider">The concrete logging provider class to register.</typeparam>
        /// <typeparam name="TOptions">The options type used to configure the provider.</typeparam>
        /// <param name="services">The service collection to add the provider and its dependencies to.</param>
        /// <param name="configure">
        /// An optional delegate to configure the provider's options. If not provided, default values will be used.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/> instance for chaining.</returns>
        /// <remarks>
        /// This method registers:
        /// <list type="bullet">
        /// <item><description>The <typeparamref name="TProvider"/> as itself and as <see cref="ILogProvider"/>.</description></item>
        /// <item><description>The <see cref="ILogProviderDescriptor"/> generated from the provider and its options.</description></item>
        /// <item><description>The <typeparamref name="TOptions"/> as configurable <c>IOptions&lt;T&gt;</c>.</description></item>
        /// </list>
        /// The provider descriptor is also added to the <see cref="LogProviderDescriptorCollection"/> for runtime lookup.
        /// </remarks>
        public static IServiceCollection AddLogProvider<TProvider, TOptions>(this IServiceCollection services, Action<TOptions> configure = default)
            where TProvider : class, ILogProvider
            where TOptions : class, ILogProviderOptions, new() => services.AddSingleton<TProvider>()
                                                                          .AddSingleton<ILogProvider>(p => p.GetRequiredService<TProvider>())
                                                                          .AddSingleton<ILogProviderDescriptor>(p =>
                                                                          {
                                                                              var provider = p.GetRequiredService<TProvider>();
                                                                              var options = p.GetRequiredService<IOptions<TOptions>>().Value;
                                                                              var descriptor = new LogProviderDescriptor(provider, options);
                                                                              p.GetRequiredService<LogProviderDescriptorCollection>().Add<TProvider>(descriptor);
                                                                              return descriptor;
                                                                          })
                                                                          .AddOptions(configure);

        /// <summary>
        /// Logs a message at the Trace log level.
        /// </summary>
        /// <param name="logService">The log service to log the message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        public static void LogTrace(this ILogService logService,
                                    Exception exception,
                                    Type type = default,
                                    [CallerFilePath] string fileName = default,
                                    [CallerMemberName] string memberName = default) =>
            logService.Log(TLogCategory.Trace, exception, type, fileName, memberName);

        /// <summary>
        /// Logs a message at the Trace log level.
        /// </summary>
        /// <param name="logService">The log service to log the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="attributes">Optional attributes related to the message. Default is <c>null</c>.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        public static void LogTrace(this ILogService logService,
                                    string message,
                                    string attributes = default,
                                    Type type = default,
                                    [CallerFilePath] string fileName = default,
                                    [CallerMemberName] string memberName = default) =>
            logService.Log(TLogCategory.Trace, message, attributes, type, fileName, memberName);

        /// <summary>
        /// Logs a message at the Debug log level.
        /// </summary>
        /// <param name="logService">The log service to log the message.</param>
        /// <param name="message">The message to log. Default is <c>null</c>.</param>
        /// <param name="attributes">Optional attributes related to the message. Default is <c>null</c>.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        public static void LogDebug(this ILogService logService,
                                    string message = default,
                                    string attributes = default,
                                    Type type = default,
                                    [CallerFilePath] string fileName = default,
                                    [CallerMemberName] string memberName = default) =>
            logService.Log(TLogCategory.Debug, message, attributes, type, fileName, memberName);

        /// <summary>
        /// Logs a message at the Info log level.
        /// </summary>
        /// <param name="logService">The log service to log the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="attributes">Optional attributes related to the message. Default is <c>null</c>.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        public static void LogInfo(this ILogService logService,
                                   string message,
                                   string attributes = default,
                                   Type type = default,
                                   [CallerFilePath] string fileName = default,
                                   [CallerMemberName] string memberName = default) =>
            logService.Log(TLogCategory.Info, message, attributes, type, fileName, memberName);

        /// <summary>
        /// Logs a message at the Warning log level.
        /// </summary>
        /// <param name="logService">The log service to log the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="attributes">Optional attributes related to the message. Default is <c>null</c>.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        public static void LogWarning(this ILogService logService,
                                      string message,
                                      string attributes = default,
                                      Type type = default,
                                      [CallerFilePath] string fileName = default,
                                      [CallerMemberName] string memberName = default) =>
            logService.Log(TLogCategory.Warning, message, attributes, type, fileName, memberName);

        /// <summary>
        /// Logs an exception at the Warning log level.
        /// </summary>
        /// <param name="logService">The log service to log the exception.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        public static void LogWarning(this ILogService logService,
                                      Exception exception,
                                      Type type = default,
                                      [CallerFilePath] string fileName = default,
                                      [CallerMemberName] string memberName = default) =>
            logService.Log(TLogCategory.Warning, exception, type, fileName, memberName);

        /// <summary>
        /// Logs an exception at the Error log level.
        /// </summary>
        /// <param name="logService">The log service to log the exception.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        public static void LogError(this ILogService logService,
                                    Exception exception,
                                    Type type = default,
                                    [CallerFilePath] string fileName = default,
                                    [CallerMemberName] string memberName = default) =>
            logService.Log(TLogCategory.Error, exception, type, fileName, memberName);

        /// <summary>
        /// Creates a task start/stop log for timing the execution of a task.
        /// </summary>
        /// <param name="logService">The log service to log the start and stop times of the task.</param>
        /// <param name="attributes">Optional attributes related to the task. Default is <c>null</c>.</param>
        /// <param name="type">The type from which the log originates. Default is <c>null</c>.</param>
        /// <param name="fileName">The file path of the calling method. Default is <c>null</c>.</param>
        /// <param name="memberName">The name of the calling method. Default is <c>null</c>.</param>
        /// <returns>An IDisposable that stops the task timer when disposed.</returns>
        public static IDisposable LogStartStop(this ILogService logService,
                                               string attributes = default,
                                               Type type = default,
                                               [CallerFilePath] string fileName = default,
                                               [CallerMemberName] string memberName = default) =>
            new StartStopRegistration(
                () => logService.Log(TLogCategory.StartStop,
                                     "Task Started",
                                     attributes,
                                     type,
                                     fileName,
                                     memberName),
                elapsed => logService.Log(TLogCategory.StartStop,
                                          "Task Stopped",
                                          $"{attributes}{(string.IsNullOrWhiteSpace(attributes) ? string.Empty : " - ")}Elapsed='{TimeSpan.FromMilliseconds(elapsed).ToReadableTime()}'",
                                          type,
                                          fileName,
                                          memberName));
    }
}
