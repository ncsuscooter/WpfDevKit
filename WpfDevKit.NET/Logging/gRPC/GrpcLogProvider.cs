// Copyright 2024 Veeco Instruments, Inc.  All rights reserved.  Distribution prohibited.

﻿using Discovery.Library.Common.Locator;
using Grpc.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Discovery.Library.Common.Logging.gRPC;

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
