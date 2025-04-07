#if NETCOREAPP

using Grpc.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WpfDevKit.Logging.gRPC
{
    [DebuggerStepThrough]
    internal class GrpcLogProvider(ILocatorService locator, ILogService logger) : SubscriberGrpc.SubscriberGrpcBase, ILogProvider
    {
        private readonly TaskCompletionSource source = new();
        private readonly LogMetrics<GrpcLogProvider> metrics = new();
        private IServerStreamWriter<LogMessageGrpc> responseStream;
        private ServerCallContext context;
        private SubscribeRequestGrpc request;
        private TLogCategory enabledCategories;
        public ILogMetrics Metrics => metrics;
        TLogCategory ILogProvider.DisabledCategories => TLogCategory.None;
        TLogCategory ILogProvider.EnabledCategories
        {
            get
            {
                if (enabledCategories == TLogCategory.None && request is not null)
                    enabledCategories = request.Categories.Aggregate(TLogCategory.None, (result, category) => result |= category.FromGrpc());
                return enabledCategories;
            }
            set => throw new NotImplementedException();
        }
        async ValueTask ILogProvider.LogAsync(LogMessage logMessage)
        {
            try
            {
                if (context.CancellationToken.IsCancellationRequested)
                    return;
                using var disposable = metrics.StartStop(logMessage);
                await responseStream.WriteAsync(logMessage.ToGrpc(), context.CancellationToken);
            }
            catch
            {
                source.TrySetCanceled();
                throw;
            }
        }
        public override async Task Subscribe(SubscribeRequestGrpc request,
                                             IServerStreamWriter<LogMessageGrpc> responseStream,
                                             ServerCallContext context)
        {
            if (!Guid.TryParse(request.RequestGuid, out var g))
                return;
            logger.LogInfo("Subscribe", $"Guid='{g}' - Host='{context.Host}'");
            try
            {
                this.request = request;
                this.context = context;
                this.responseStream = responseStream;
                locator.AddLogProvider(logger, this, g);
                using var registration = context.CancellationToken.Register(source.SetResult);
                await source.Task;
            }
            catch (OperationCanceledException)
            {
                // INTENTIONALLY LEFT EMPTY
            }
            catch (Exception ex)
            {
                source.TrySetCanceled();
                logger.LogError(ex);
            }
            finally
            {
                locator.RemoveLogProvider(logger, this, g);
            }
            logger.LogInfo("Unsubscribe", $"Guid='{g}' - Host='{context.Host}'");
        }
    }

    /// <summary>
    /// Provides a gRPC-based log provider that streams log messages to one or more remote clients.
    /// </summary>
    [DebuggerStepThrough]
    internal class GrpcLogProvider : SubscriberGrpc.SubscriberGrpcBase, ILogProvider
    {
        private readonly ILocatorService locator;
        private readonly ILogService logger;
        private readonly LogMetrics<GrpcLogProvider> metrics = new();
        private readonly ConcurrentDictionary<Guid, GrpcLogStreamContext> subscribers = new();

        public GrpcLogProvider(ILocatorService locator, ILogService logger)
        {
            this.locator = locator;
            this.logger = logger;
        }

        public ILogMetrics Metrics => metrics;
        public TLogCategory DisabledCategories => TLogCategory.None;
        public TLogCategory EnabledCategories => subscribers.Values.Aggregate(TLogCategory.None, (all, next) => all | next.EnabledCategories);

        public async ValueTask LogAsync(LogMessage logMessage)
        {
            using var _ = metrics.StartStop(logMessage);
            foreach (var (id, sub) in subscribers)
            {
                if (sub.Context.CancellationToken.IsCancellationRequested)
                {
                    RemoveSubscriber(id);
                    continue;
                }
                try
                {
                    await sub.Stream.WriteAsync(logMessage.ToGrpc(), sub.Context.CancellationToken);
                }
                catch
                {
                    RemoveSubscriber(id);
                }
            }
        }

        public override async Task Subscribe(SubscribeRequestGrpc request, IServerStreamWriter<LogMessageGrpc> stream, ServerCallContext context)
        {
            if (!Guid.TryParse(request.RequestGuid, out var id)) return;
            if (request.AuthToken != "secret-token") return;

            var categories = request.Categories.Aggregate(TLogCategory.None, (acc, c) => acc | c.FromGrpc());
            var subContext = new GrpcLogStreamContext(id, stream, context, categories);

            subscribers[id] = subContext;
            locator.AddLogProvider(logger, this, id);

            logger.LogInfo("Subscribe", $"Subscriber: {id} from {context.Host}");
            try
            {
                await subContext.CompletionSource.Task;
            }
            catch (OperationCanceledException) { }
            finally
            {
                RemoveSubscriber(id);
                logger.LogInfo("Unsubscribe", $"Subscriber: {id} from {context.Host}");
            }
        }

        private void RemoveSubscriber(Guid id)
        {
            if (subscribers.TryRemove(id, out var sub))
            {
                locator.RemoveLogProvider(logger, this, id);
                sub.CompletionSource.TrySetResult();
            }
        }
    }

    /// <summary>
    /// Represents an active gRPC log stream subscription.
    /// </summary>
    [DebuggerStepThrough]
    internal class GrpcLogStreamContext
    {
        public Guid Id { get; }
        public IServerStreamWriter<LogMessageGrpc> Stream { get; }
        public ServerCallContext Context { get; }
        public TLogCategory EnabledCategories { get; }
        public TaskCompletionSource CompletionSource { get; } = new();

        public GrpcLogStreamContext(Guid id, IServerStreamWriter<LogMessageGrpc> stream, ServerCallContext context, TLogCategory enabledCategories)
        {
            Id = id;
            Stream = stream;
            Context = context;
            EnabledCategories = enabledCategories;
        }
    }

    [DebuggerStepThrough]
    internal static class GrpcLogExtensions
    {
        private static string ToGrpcValue(this string value) => value ?? string.Empty;
        public static LogMessageGrpc ToGrpc(this LogMessage message) => new()
        {
            ApplicationName = message.ApplicationName.ToGrpcValue(),
            ApplicationVersion = message.ApplicationVersion.ToString().ToGrpcValue(),
            Attributes = message.Attributes.ToGrpcValue(),
            Category = message.Category.ToGrpc(),
            ClassName = message.ClassName.ToGrpcValue(),
            ExceptionLevel = message.ExceptionLevel,
            ExceptionStackTrace = message.ExceptionStackTrace.ToGrpcValue(),
            Index = message.Index,
            MachineName = message.MachineName.ToGrpcValue(),
            Message = message.Message.ToGrpcValue(),
            MethodName = message.MethodName.ToGrpcValue(),
            ThreadId = message.ThreadId,
            Timestamp = Timestamp.FromDateTime(message.Timestamp.ToUniversalTime()),
            UserName = message.UserName.ToGrpcValue()
        };
        public static LogMessage FromGrpc(this LogMessageGrpc message) => new(message.Index,
                                                                              message.Timestamp.ToDateTime().ToLocalTime(),
                                                                              message.MachineName,
                                                                              message.UserName,
                                                                              message.ApplicationName,
                                                                              Version.Parse(message.ApplicationVersion),
                                                                              message.ClassName,
                                                                              message.MethodName,
                                                                              message.ThreadId,
                                                                              message.Category.FromGrpc(),
                                                                              message.Message,
                                                                              message.Attributes,
                                                                              message.ExceptionLevel,
                                                                              message.ExceptionStackTrace);
        public static TLogCategoryGrpc ToGrpc(this TLogCategory category) => category switch
        {
            TLogCategory.Trace => TLogCategoryGrpc.Trace,
            TLogCategory.Debug => TLogCategoryGrpc.Debug,
            TLogCategory.Info => TLogCategoryGrpc.Info,
            TLogCategory.StartStop => TLogCategoryGrpc.StartStop,
            TLogCategory.Warning => TLogCategoryGrpc.Warning,
            TLogCategory.Error => TLogCategoryGrpc.Error,
            TLogCategory.Fatal => TLogCategoryGrpc.Fatal,
            _ => TLogCategoryGrpc.None,
        };
        public static TLogCategory FromGrpc(this TLogCategoryGrpc category) => category switch
        {
            TLogCategoryGrpc.Trace => TLogCategory.Trace,
            TLogCategoryGrpc.Debug => TLogCategory.Debug,
            TLogCategoryGrpc.Info => TLogCategory.Info,
            TLogCategoryGrpc.StartStop => TLogCategory.StartStop,
            TLogCategoryGrpc.Warning => TLogCategory.Warning,
            TLogCategoryGrpc.Error => TLogCategory.Error,
            TLogCategoryGrpc.Fatal => TLogCategory.Fatal,
            _ => TLogCategory.None,
        };
    }

    /// <summary>
    /// A gRPC client that connects to a remote log server and receives live log messages.
    /// </summary>
    [DebuggerStepThrough]
    public class GrpcLogClient : IDisposable
    {
        private readonly Task task;
        private readonly CancellationTokenSource source = new();
        private readonly CancellationTokenRegistration registration;
        public string Host { get; }
        public int Port { get; }

        public GrpcLogClient(string host, int port, List<TLogCategory> categories, string authToken, Action<LogMessage> onMessage, CancellationToken cancelToken = default)
        {
            Host = host;
            Port = port;
            registration = cancelToken.Register(source.Cancel);
            task = Task.Run(async () =>
            {
                using var channel = GrpcChannel.ForAddress($"http://{host}:{port}");
                var client = new SubscriberGrpc.SubscriberGrpcClient(channel);
                var request = new SubscribeRequestGrpc
                {
                    RequestGuid = Guid.NewGuid().ToString(),
                    AuthToken = authToken
                };
                request.Categories.AddRange(categories.Select(c => c.ToGrpc()));
                try
                {
                    var call = client.Subscribe(request, cancellationToken: source.Token);
                    while (await call.ResponseStream.MoveNext(source.Token).ConfigureAwait(false))
                        onMessage(call.ResponseStream.Current.FromGrpc());
                }
                catch (OperationCanceledException) { }
                catch (RpcException) { }
                finally
                {
                    await channel.ShutdownAsync();
                }
            }, source.Token);
        }

        public void Dispose()
        {
            registration.Dispose();
            source.Cancel();
            readerTask.Wait();
        }
    }
}

#endif
