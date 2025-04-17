using System;
using System.Collections.Generic;

namespace WpfDevKit.Factory
{
    internal interface IObjectResolver
    {
        bool CanResolve(Type type);
        object Resolve(Type type, HashSet<Type> stack);
    }
}
