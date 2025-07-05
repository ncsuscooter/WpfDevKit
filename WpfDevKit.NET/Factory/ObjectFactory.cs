using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace WpfDevKit.Factory
{
    /// <summary>
    /// Provides runtime creation of object instances with support for constructor injection,
    /// property injection via <see cref="ResolvableAttribute"/>, and optional parameter matching.
    /// </summary>
    [DebuggerStepThrough]
    internal class ObjectFactory : IObjectFactory
    {
        private readonly IObjectResolver resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFactory"/> class with the given service provider.
        /// </summary>
        /// <param name="resolver">The object resolver used for resolving dependencies.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
        public ObjectFactory(IObjectResolver resolver) => this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        /// <inheritdoc/>
        /// <remarks>
        /// Constructor arguments provided by the caller take precedence over resolved services when matching parameters.
        /// </remarks>
        public T Create<T>(params object[] args) where T : class => (T)Create(typeof(T), args);

        /// <inheritdoc/>
        /// <remarks>
        /// Constructor arguments provided by the caller take precedence over resolved services when matching parameters.
        /// </remarks>
        public object Create(Type type, params object[] args) =>
            Create(type, new HashSet<Type>(), args, () => type.GetConstructors()
                                                              .Select(ctor => new { Constructor = ctor, Score = ScoreConstructor(ctor, args) })
                                                              .Where(x => x.Score >= 0)
                                                              .OrderByDescending(x => x.Score)
                                                              .ThenByDescending(x => x.Constructor.GetParameters().Length)
                                                              .Select(x => x.Constructor)
                                                              .FirstOrDefault());

        /// <summary>
        /// Entry method to find the appropriate constructor (using the provided delegate), it's parameters, and the type's properties 
        /// marked with <see cref="ResolvableAttribute"/>. All used to create and initialize an object instance with dependency and property injection.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="stack">A set used to detect circular dependencies during recursive resolution.</param>
        /// <param name="args">Optional arguments to be matched against constructor parameters.</param>
        /// <param name="getConstructor">A delegate that returns the most appropriate constructor for instantiation.</param>
        /// <returns>A fully constructed and property-injected object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="type"/> is not a class.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a circular dependency is detected or when no suitable constructor can be resolved.
        /// </exception>>
        internal object Create(Type type, HashSet<Type> stack, object[] args, Func<ConstructorInfo> getConstructor)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!type.IsClass) throw new ArgumentException("Type must be a class", nameof(type));
            if (stack == null) throw new ArgumentNullException(nameof(stack));
            if (!stack.Add(type)) throw new InvalidOperationException($"Circular dependency detected for type {type.Name}.");
            try
            {
                var constructor = getConstructor() ?? throw new InvalidOperationException($"No suitable constructor found for {type.Name}.");
                var parameters = constructor.GetParameters();
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     .Where(p => p.IsDefined(typeof(ResolvableAttribute), inherit: true) && p.CanWrite);
                return Create(constructor, parameters, properties, stack, args);
            }
            finally
            {
                stack.Remove(type);
            }
        }
        /// <summary>
        /// Core instantiation logic used to create and initialize an object instance with dependency and property injection.
        /// </summary>
        /// <param name="constructor">The constructor to be used for instantiation.</param>
        /// <param name="parameters">The constructor's parameters to be used for instantiation.</param>
        /// <param name="properties">The type's properties using the <see cref="ResolvableAttribute"/> for DI.</param>
        /// <param name="args">Arguments to be matched against constructor parameters.</param>
        /// <returns>A fully constructed and property-injected object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="type"/> is not a class.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected.</exception>>
        internal object Create(ConstructorInfo constructor, ParameterInfo[] parameters, IEnumerable<PropertyInfo> properties, HashSet<Type> stack, object[] args)
        {
            var arguments = new object[parameters.Length];
            var usedArgs = new HashSet<int>();

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                bool resolved = false;

                if (args != null)
                {
                    if (i < args.Length)
                    {
                        var arg = args[i];
                        if (arg == null && !param.ParameterType.IsValueType ||
                            arg != null && param.ParameterType.IsInstanceOfType(arg))
                        {
                            usedArgs.Add(i);
                            arguments[i] = arg;
                            resolved = true;
                        }
                    }

                    if (!resolved)
                    {
                        for (int j = 0; j < args.Length; j++)
                        {
                            if (usedArgs.Contains(j)) continue;
                            var arg = args[j];
                            if (arg == null && !param.ParameterType.IsValueType ||
                                arg != null && param.ParameterType.IsInstanceOfType(arg))
                            {
                                usedArgs.Add(j);
                                arguments[j] = arg;
                                resolved = true;
                                break;
                            }
                        }
                    }
                }

                if (!resolved && resolver.CanResolve(param.ParameterType))
                {
                    arguments[i] = resolver.Resolve(param.ParameterType, stack);
                    resolved = true;
                }
                if (!resolved && param.HasDefaultValue)
                {
                    arguments[i] = param.DefaultValue;
                    resolved = true;
                }
                if (!resolved)
                    throw new InvalidOperationException($"Cannot resolve parameter '{param.Name}' of type '{param.ParameterType.Name}'.");
            }

            var instance = constructor.Invoke(arguments);

            foreach (var prop in properties)
            {
                if (resolver.CanResolve(prop.PropertyType))
                    prop.SetValue(instance, resolver.Resolve(prop.PropertyType, stack));
                else
                    Debug.WriteLine($"[Inject] Warning: No service ({prop.PropertyType.Name}) found for property ({prop.Name}).");
            }

            return instance;
        }

        /// <summary>
        /// Scores a constructor based on how well its parameters match the provided arguments,
        /// services available from the provider, or default values.
        /// </summary>
        /// <param name="ctor">The constructor to score.</param>
        /// <param name="args">Optional user-supplied arguments.</param>
        /// <returns>
        /// A non-negative integer score representing the quality of the match.
        /// Returns -1 if any parameter cannot be satisfied.
        /// </returns>
        private int ScoreConstructor(ConstructorInfo ctor, object[] args)
        {
            int score = 0;
            var parameters = ctor.GetParameters();
            var usedArgs = new HashSet<int>();
            foreach (var param in parameters)
            {
                bool matched = false;
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (usedArgs.Contains(i))
                            continue;
                        var arg = args[i];
                        if (arg == null && !param.ParameterType.IsValueType ||
                            arg != null && param.ParameterType.IsInstanceOfType(arg))
                        {
                            score += 5;
                            matched = true;
                            usedArgs.Add(i);
                            break;
                        }
                    }
                }
                if (!matched && resolver.CanResolve(param.ParameterType))
                {
                    score += 3;
                    matched = true;
                }
                if (!matched && param.HasDefaultValue)
                {
                    score += 1;
                    matched = true;
                }
                if (!matched)
                    return -1;
            }
            return score;
        }
    }
}

// TODO: CACHE_CTOR_INFO
// Constructor selection and scoring uses reflection on every Create() call.
// Consider caching resolved ConstructorInfo (e.g., via ConcurrentDictionary<Type, ConstructorInfo>) to improve performance.
