using System;
using System.Diagnostics;

namespace WpfDevKit.Factory
{
    /// <summary>
    /// Attribute used to mark classes or services as resolvable through the custom
    /// dependency injection system at runtime.
    /// </summary>
    [DebuggerStepThrough]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ResolvableAttribute : Attribute { }
}
