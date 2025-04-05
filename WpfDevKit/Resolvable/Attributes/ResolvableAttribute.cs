using System;

namespace WpfDevKit.Mvvm
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ResolvableAttribute : Attribute { }
}
