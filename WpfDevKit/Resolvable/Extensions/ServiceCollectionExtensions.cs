using System;
using System.Linq;
using System.Reflection;
using WpfDevKit.DependencyInjection.Interfaces;

namespace WpfDevKit.Mvvm
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResolvableFromAssembly(this IServiceCollection services, Assembly assembly, Action<string> log = null) => 
            services.AddResolvableFromAssemblies(new[] { assembly }, log);

        public static IServiceCollection AddResolvableFromAssemblies(this IServiceCollection services, Assembly[] assemblies, Action<string> log = null)
        {
            var collection = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ResolvableAttribute>() != null)
                .ToList();
            foreach (var item in collection)
            {
                services.AddTransient(item, item);
                log?.Invoke($"Object registered: {item.FullName}");
            }
            return services;
        }
    }
}
