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
                    .AddSingleton<LogProviderCollection>()
                    .AddSingleton<LogBackgroundService>()
                    .AddSingleton<ILogProviderCollection>(p => p.GetService<LogProviderCollection>())
                    .AddSingleton<ILogMetricsReader>(p => p.GetService<LogMetrics>())
                    .AddSingleton<ILogService>(p => p.GetService<LogService>());

        /// <summary>
        /// Registers the <see cref="MemoryLogProvider"/> with optional configuration.
        /// </summary>
        /// <param name="services">The service collection to add the provider to.</param>
        /// <param name="configure">An optional delegate to configure the provider options.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMemoryLogProvider(this IServiceCollection services, Action<MemoryLogProviderOptions> configure = default) =>
            services.AddSingleton<MemoryLogProvider>().AddOptions(configure);

        /// <summary>
        /// Registers the <see cref="ConsoleLogProvider"/> with optional configuration.
        /// </summary>
        /// <param name="services">The service collection to add the provider to.</param>
        /// <param name="configure">An optional delegate to configure the provider options.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddConsoleLogProvider(this IServiceCollection services, Action<ConsoleLogProviderOptions> configure = default) =>
            services.AddSingleton<ConsoleLogProvider>().AddOptions(configure);

        /// <summary>
        /// Registers the <see cref="DatabaseLogProvider"/> with optional configuration.
        /// </summary>
        /// <param name="services">The service collection to add the provider to.</param>
        /// <param name="configure">An optional delegate to configure the provider options.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDatabaseLogProvider(this IServiceCollection services, Action<DatabaseLogProviderOptions> configure = default) =>
            services.AddSingleton<DatabaseLogProvider>().AddOptions(configure);

        /// <summary>
        /// Registers the <see cref="UserLogProvider"/> and exposes it via the <see cref="IUserLogProvider"/> interface,
        /// with optional configuration.
        /// </summary>
        /// <param name="services">The service collection to add the provider to.</param>
        /// <param name="configure">An optional delegate to configure the provider options.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddUserLogProvider(this IServiceCollection services, Action<UserLogProviderOptions> configure = default) =>
            services.AddSingleton<UserLogProvider>()
                    .AddSingleton<IUserLogProvider>(p => p.GetService<UserLogProvider>())
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
