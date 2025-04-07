// Copyright 2024 Veeco Instruments, Inc.  All rights reserved.  Distribution prohibited.

﻿using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Discovery.Library.Common.Logging.gRPC;

[DebuggerStepThrough]
public class GrpcLogClient : IDisposable
{
    private readonly Task task;
    private readonly CancellationTokenSource source = new();
    private readonly CancellationTokenRegistration registration;
    public string Host { get; }
    public int Port { get; }
    public GrpcLogClient(string host, int port, List<TLogCategory> categories, Action<LogMessage> action, CancellationToken cancellationToken = default)
    {
        Host = host;
        Port = port;
        registration = cancellationToken.Register(source.Cancel);
        task = Task.Run(async () =>
        {
            using var channel = GrpcChannel.ForAddress($"http://{host}:{port}");
            try
            {
                var request = new SubscribeRequestGrpc()
                {
                    RequestGuid = Guid.NewGuid().ToString()
                };
                foreach (var item in categories)
                    request.Categories.Add(item.ToGrpc());
                var client = new SubscriberGrpc.SubscriberGrpcClient(channel);
                var call = client.Subscribe(request, default, default, source.Token);
                while (await call.ResponseStream.MoveNext(source.Token).ConfigureAwait(false))
                    action(call.ResponseStream.Current.FromGrpc());
            }
            catch (OperationCanceledException)
            {
                // INTENTIONALLY LEFT EMPTY
            }
            catch (RpcException)
            {
                // INTENTIONALLY LEFT EMPTY
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }, source.Token);
    }
    public void Dispose()
    {
        registration.Unregister();
        registration.Dispose();
        source.Cancel();
        task.Wait();
    }
}
