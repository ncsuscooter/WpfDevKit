using System;

namespace WpfDevKit.Core
{
    /// <summary>
    /// Attribute used to mark classes or services as resolvable through the custom
    /// dependency injection system at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ResolvableAttribute : Attribute { }
}
