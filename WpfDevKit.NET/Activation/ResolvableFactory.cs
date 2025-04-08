using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WpfDevKit.Activation
{
    /// <summary>
    /// Default implementation of <see cref="IResolvableFactory"/> that resolves
    /// and creates instances of classes using constructor and property injection.
    /// Supports dynamic parameters and attributes for fine-grained control.
    /// </summary>
    internal class ResolvableFactory : IResolvableFactory
    {
        //private readonly ConcurrentDictionary<ConstructorCacheKey, ConstructorInfo> constructorCache = new ConcurrentDictionary<ConstructorCacheKey, ConstructorInfo>();
        //private readonly ConcurrentDictionary<Type, List<PropertyInfo>> propertiesCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvableFactory"/> class
        /// using the specified service provider.
        /// </summary>
        /// <param name="provider">The service provider used to resolve dependencies.</param>
        public ResolvableFactory(IServiceProvider provider) => this.provider = provider;

        /// <summary>
        /// Creates an instance of the specified class type, resolving dependencies
        /// via constructor and property injection.
        /// </summary>
        /// <typeparam name="TClass">The type of class to instantiate.</typeparam>
        /// <param name="args">Optional parameters to assist with constructor resolution.</param>
        /// <returns>A fully constructed instance of <typeparamref name="TClass"/>.</returns>
        public TClass Create<TClass>(params object[] args) where TClass : class => (TClass)Create(typeof(TClass), args);

        /// <summary>
        /// Creates an instance of the specified type using the most suitable constructor
        /// and resolves any [Resolvable]-decorated properties after instantiation.
        /// </summary>
        /// <param name="type">The type to create. Must be a non-abstract class.</param>
        /// <param name="args">Optional parameters used to assist with constructor resolution.</param>
        /// <returns>A new instance of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided <paramref name="type"/> is not a class.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no public constructor is found or dependencies cannot be resolved.</exception>
        public object Create(Type type, params object[] args)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!type.IsClass) throw new ArgumentException("Type must be a class", nameof(type));

            //var cacheKey = new ConstructorCacheKey(type, args);
            var constructor = ResolveConstructor(type, args); //constructorCache.GetOrAdd(cacheKey, _ => ResolveConstructor(type, args));
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];
            var usedArgs = new HashSet<int>();

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                bool resolved = false;

                // 1. Try match by position
                if (args != null && i < args.Length)
                {
                    var arg = args[i];
                    if ((arg == null && !param.ParameterType.IsValueType) ||
                        (arg != null && param.ParameterType.IsInstanceOfType(arg)))
                    {
                        arguments[i] = arg;
                        usedArgs.Add(i);
                        resolved = true;
                    }
                }

                // 2. Try match by unused argument type
                if (!resolved && args != null)
                {
                    for (int j = 0; j < args.Length; j++)
                    {
                        if (usedArgs.Contains(j)) continue;
                        var arg = args[j];
                        if ((arg == null && !param.ParameterType.IsValueType) ||
                            (arg != null && param.ParameterType.IsInstanceOfType(arg)))
                        {
                            arguments[i] = arg;
                            usedArgs.Add(j);
                            resolved = true;
                            break;
                        }
                    }
                }

                // 3. Try DI
                if (!resolved)
                {
                    var service = provider.GetService(param.ParameterType);
                    if (service != null)
                    {
                        arguments[i] = service;
                        resolved = true;
                    }
                }

                // 4. Try default
                if (!resolved && param.HasDefaultValue)
                {
                    arguments[i] = param.DefaultValue;
                    resolved = true;
                }

                // 5. Fail if unresolved
                if (!resolved)
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve parameter '{param.Name}' of type '{param.ParameterType.Name}'.");
                }
            }

            var result = constructor.Invoke(arguments);
            var properties = ResolveProperties(type); //propertiesCache.GetOrAdd(type, ResolveProperties);

            foreach (var prop in properties)
            {
                var service = provider.GetService(prop.PropertyType);
                if (service != null)
                {
                    prop.SetValue(result, service);
                }
                else
                {
                    provider.GetService<InternalLogger>()?.LogMessage?.Invoke($"[Inject] Warning: No service found for property",
                                                                              $"Property Name: {prop.Name} - Property Type: {prop.PropertyType.Name} Service: {type.Name}.",
                                                                              default);
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the best-fit constructor for a type, preferring the one with the most resolvable parameters.
        /// </summary>
        /// <param name="type">The type for which to resolve a constructor.</param>
        /// <param name="args">Parameters used to assist with constructor resolution.</param>
        /// <returns>The selected constructor.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no suitable constructor is found.</exception>
        private ConstructorInfo ResolveConstructor(Type type, object[] args) =>
            type.GetConstructors()
                .Select(ctor => new { Constructor = ctor, Score = ScoreConstructor(ctor, args) })
                .Where(x => x.Score >= 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Constructor.GetParameters().Length)
                .Select(x => x.Constructor)
                .FirstOrDefault() ?? throw new InvalidOperationException($"No suitable constructor found for {type.Name}.");

        /// <summary>
        /// Computes a weighted score for a given constructor based on how well its parameters
        /// can be resolved using the provided arguments, the service provider, or optional values.
        /// </summary>
        /// <param name="ctor">The constructor to evaluate.</param>
        /// <param name="args">Optional user-supplied arguments used to assist with parameter resolution.</param>
        /// <returns>
        /// A non-negative integer representing the suitability of the constructor.
        /// Returns -1 if the constructor has any parameters that cannot be resolved.
        /// </returns>
        private int ScoreConstructor_old(ConstructorInfo ctor, object[] args)
        {
            int score = 0;
            foreach (var param in ctor.GetParameters())
            {
                bool matched = false;

                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        if (arg == null && !param.ParameterType.IsValueType)
                        {
                            matched = true;
                            break;
                        }
                        if (arg != null && param.ParameterType.IsInstanceOfType(arg))
                        {
                            matched = true;
                            break;
                        }
                    }
                }

                if (matched)
                    score += 2;
                else if (provider.GetService(param.ParameterType) != null)
                    score += 3;
                else if (param.IsOptional)
                    score += 1;
                else
                    return -1; // Cannot satisfy this constructor
            }
            return score;
        }

        private int ScoreConstructor(ConstructorInfo ctor, object[] args)
        {
            int score = 0;
            var parameters = ctor.GetParameters();
            var usedArgs = new HashSet<int>();

            foreach (var param in parameters)
            {
                bool matched = false;

                // Match by unused args
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (usedArgs.Contains(i)) continue;
                        var arg = args[i];

                        if ((arg == null && !param.ParameterType.IsValueType) ||
                            (arg != null && param.ParameterType.IsInstanceOfType(arg)))
                        {
                            score += 5; // highest priority: direct match
                            usedArgs.Add(i);
                            matched = true;
                            break;
                        }
                    }
                }

                if (!matched && provider.GetService(param.ParameterType) != null)
                {
                    score += 3; // mid priority: service match
                    matched = true;
                }

                if (!matched && param.HasDefaultValue)
                {
                    score += 1; // lowest priority: optional param
                    matched = true;
                }

                if (!matched)
                    return -1; // cannot resolve this param
            }

            return score;
        }



        /// <summary>
        /// Resolves the public setters for an instance type marked with the <see cref="ResolvableAttribute"/> for dependency injection via properties.
        /// </summary>
        /// <param name="type">The type for which to resolve properties.</param>
        /// <returns>The collection of properties.</returns>
        private List<PropertyInfo> ResolveProperties(Type type) => 
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.IsDefined(typeof(ResolvableAttribute), inherit: true) && p.CanWrite)
                .ToList();
        //private readonly struct ConstructorCacheKey : IEquatable<ConstructorCacheKey>
        //{
        //    public Type TargetType { get; }
        //    public string Signature { get; }

        //    public ConstructorCacheKey(Type type, object[] args)
        //    {
        //        TargetType = type;

        //        // Serialize more details about the input args and DI context
        //        var argTypes = args?.Select(a => a?.GetType()?.FullName ?? "null") ?? Enumerable.Empty<string>();
        //        var services = type
        //            .GetConstructors()
        //            .SelectMany(c => c.GetParameters())
        //            .Select(p => p.ParameterType.FullName + ":" + (p.IsOptional ? "optional" : "required"))
        //            .Distinct()
        //            .OrderBy(x => x);

        //        Signature = string.Join("|", argTypes.Concat(services));
        //    }

        //    public bool Equals(ConstructorCacheKey other) =>
        //        TargetType == other.TargetType && Signature == other.Signature;

        //    public override bool Equals(object obj) =>
        //        obj is ConstructorCacheKey other && Equals(other);

        //    public override int GetHashCode()
        //    {
        //        int hash = TargetType.GetHashCode();
        //        hash = (hash * 397) ^ (Signature?.GetHashCode() ?? 0);
        //        return hash;
        //    }
        //}
        //private readonly struct ConstructorCacheKey2 : IEquatable<ConstructorCacheKey2>
        //{
        //    public Type TargetType { get; }
        //    public Type[] ArgTypes { get; }
        //    public ConstructorCacheKey2(Type targetType, object[] args) => 
        //        (TargetType, ArgTypes) = (targetType, args?.Select(a => a?.GetType() ?? typeof(object)).ToArray() ?? Type.EmptyTypes);
        //    public bool Equals(ConstructorCacheKey2 other)
        //    {
        //        if (TargetType != other.TargetType || ArgTypes.Length != other.ArgTypes.Length)
        //            return false;
        //        for (int i = 0; i < ArgTypes.Length; i++)
        //            if (ArgTypes[i] != other.ArgTypes[i])
        //                return false;
        //        return true;
        //    }
        //    public override bool Equals(object obj) => obj is ConstructorCacheKey2 other && Equals(other);
        //    public override int GetHashCode()
        //    {
        //        int hash = TargetType.GetHashCode();
        //        foreach (var type in ArgTypes)
        //            hash = (hash * 397) ^ (type?.GetHashCode() ?? 0);
        //        return hash;
        //    }
        //}
    }
}
