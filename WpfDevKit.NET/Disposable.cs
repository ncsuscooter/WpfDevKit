using System;
using System.Diagnostics;

namespace WpfDevKit
{
    /// <summary>
    /// A simple & reusable IDisposable that invokes the given action when disposed.
    /// </summary>
    [DebuggerStepThrough]
    public static class Disposable
    {
        public static IDisposable Create(Action disposableAction) => new StartStopRegistration(stopAction: x => disposableAction());
    }
}