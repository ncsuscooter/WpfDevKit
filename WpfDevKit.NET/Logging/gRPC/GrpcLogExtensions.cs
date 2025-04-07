// Copyright 2024 Veeco Instruments, Inc.  All rights reserved.  Distribution prohibited.

﻿using Google.Protobuf.WellKnownTypes;
using System;
using System.Diagnostics;

namespace Discovery.Library.Common.Logging.gRPC;

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
