using System.Diagnostics;
using WpfDevKit.Busy;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Factory;
using WpfDevKit.Logging;
using WpfDevKit.RemoteFileAccess;

namespace WpfDevKit
{
    [DebuggerStepThrough]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services) =>
            services.AddObjectFactory()
                    .AddBusyService()
                    .AddLoggingService()
                    .AddRemoteFileConnectionFactory();
    }
}