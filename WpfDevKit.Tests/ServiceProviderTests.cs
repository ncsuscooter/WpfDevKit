// Unit tests for ServiceCollection and ServiceProvider behavior

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Activation;
using System.Linq;
using static WpfDevKit.Tests.DependencyInjection.ServiceProviderTests;

namespace WpfDevKit.Tests.DependencyInjection
{
    [TestClass]
    public class ServiceProviderTests
    {
        private IServiceCollection services;

        [TestInitialize]
        public void Setup()
        {
            services = new ServiceCollection();
        }

        [TestMethod]
        public void AddSingleton_ResolvesSameInstance()
        {
            services.AddSingleton<ITestService, TestService>();
            var provider = services.Build();

            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void AddTransient_ResolvesDifferentInstances()
        {
            services.AddTransient<ITestService, TestService>();
            var provider = services.Build();

            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void AddSingletonFactory_ResolvesSameInstance()
        {
            services.AddSingleton<ITestService>(_ => new TestService());
            var provider = services.Build();

            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void AddTransientFactory_ResolvesNewInstance()
        {
            services.AddTransient<ITestService>(_ => new TestService());
            var provider = services.Build();

            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void AddOptions_RegistersDefaultOptions()
        {
            services.AddOptions<MyOptions>();
            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.IsNotNull(options);
            Assert.IsNotNull(options.Value);
            Assert.AreEqual("default", options.Value.Name);
        }

        [TestMethod]
        public void AddOptionsWithConfig_SetsConfiguredValue()
        {
            services.AddOptions<MyOptions>(opt => opt.Name = "configured");
            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual("configured", options.Value.Name);
        }

        [TestMethod]
        public void Build_ThrowsOnDuplicateBuild()
        {
            services.AddSingleton<ITestService, TestService>();
            var provider1 = services.Build();
            Assert.ThrowsException<InvalidOperationException>(() => services.AddSingleton<ITestService, TestService>());
        }

        [TestMethod]
        public void GetRequiredService_ThrowsIfMissing()
        {
            var provider = services.Build();
            Assert.ThrowsException<InvalidOperationException>(() => provider.GetRequiredService<ITestService>());
        }

        [TestMethod]
        public void GetService_ReturnsNullIfMissing()
        {
            var provider = services.Build();
            var result = provider.GetService<ITestService>();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EnumerableResolution_ReturnsAllRegistered()
        {
            services.AddTransient<ITestService, TestService>();
            services.AddTransient<ITestService, AnotherTestService>();
            var provider = services.Build();

            var all = provider.GetServices<ITestService>();
            var list = new List<ITestService>(all);

            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Exists(s => s is TestService));
            Assert.IsTrue(list.Exists(s => s is AnotherTestService));
        }

        [TestMethod]
        public void AddSingleton_WithSelfReference_WorksCorrectly()
        {
            services.AddSingleton<TestService>();
            var provider = services.Build();

            var a = provider.GetService<TestService>();
            var b = provider.GetService<TestService>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void AddTransient_WithSelfReference_WorksCorrectly()
        {
            services.AddTransient<TestService>();
            var provider = services.Build();

            var a = provider.GetService<TestService>();
            var b = provider.GetService<TestService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void Create_WithDifferentManualArgs_AvoidsConstructorCacheConflict()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDependency, Dependency>();
            var provider = services.Build();
            var factory = new ResolvableFactory(provider);

            var first = factory.Create<MultiConstructorClass>("manual");
            var second = factory.Create<MultiConstructorClass>();

            Assert.AreEqual("manual", first.Source);
            Assert.AreEqual("resolved", second.Source);
        }

        [TestMethod]
        public void Create_WithManualArgsAndNullValues_WorksAsExpected()
        {
            var factory = new ResolvableFactory(new ServiceCollection().Build());
            var instance = factory.Create<NullableArgumentClass>((string)null);
            Assert.IsNotNull(instance);
            Assert.IsNull(instance.Text);
        }

        [TestMethod]
        public void Singleton_ReturnsSameInstance()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService, ServiceImpl1>();
            var provider = services.Build();

            var a = provider.GetService<IService>();
            var b = provider.GetService<IService>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void Transient_ReturnsDifferentInstances()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, ServiceImpl1>();
            var provider = services.Build();

            var a = provider.GetService<IService>();
            var b = provider.GetService<IService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void FactoryRegistration_ProducesInstance()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService>(_ => new ServiceImpl1());
            var provider = services.Build();

            var instance = provider.GetService<IService>();
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void EnumerableResolution_ReturnsAllImplementations()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, ServiceImpl1>();
            services.AddTransient<IService, ServiceImpl2>();
            var provider = services.Build();

            var all = provider.GetServices<IService>();

            Assert.AreEqual(2, all.Count());
        }

        [TestMethod]
        public void UnregisteredService_ReturnsNull()
        {
            var services = new ServiceCollection();
            var provider = services.Build();

            var service = provider.GetService<IService>();

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RequiredService_ThrowsWhenMissing()
        {
            var services = new ServiceCollection();
            var provider = services.Build();

            provider.GetRequiredService<IService>(); // should throw
        }

        [TestMethod]
        public void SelfRegisteredType_ResolvesSuccessfully()
        {
            var services = new ServiceCollection();
            services.AddTransient<ServiceImpl1>();
            var provider = services.Build();

            var instance = provider.GetService<ServiceImpl1>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void NestedResolution_ResolvesDependenciesCorrectly()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService, ServiceImpl1>();
            services.AddTransient<DependentService>();
            var provider = services.Build();

            var result = provider.GetService<DependentService>();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Service);
        }

        [TestMethod]
        public void GetServices_WithMultipleTransients_ReturnsAllInstances()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, ServiceA>();
            services.AddTransient<IService, ServiceB>();
            var provider = services.Build();

            var all = provider.GetServices<IService>().ToList();

            Assert.AreEqual(2, all.Count);
            Assert.IsInstanceOfType(all[0], typeof(IService));
            Assert.IsInstanceOfType(all[1], typeof(IService));
        }

        [TestMethod]
        public void GetServices_WhenNoneRegistered_ReturnsEmptyEnumerable()
        {
            var services = new ServiceCollection();
            var provider = services.Build();

            var result = provider.GetServices<IService>();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetServices_WithSingletonAndTransient_ResolvesBoth()
        {
            var services = new ServiceCollection();
            var singletonInstance = new ServiceA();
            services.AddSingleton<IService>(_ => singletonInstance);
            services.AddTransient<IService, ServiceB>();
            var provider = services.Build();

            var all = provider.GetServices<IService>().ToList();

            Assert.AreEqual(2, all.Count);
            Assert.AreSame(singletonInstance, all[0]);
            Assert.IsInstanceOfType(all[1], typeof(ServiceB));
        }

        [TestMethod]
        public void GetServices_ArrayCasting_WorksAsExpected()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, ServiceA>();
            services.AddTransient<IService, ServiceB>();
            var provider = services.Build();

            var raw = provider.GetService(typeof(IEnumerable<IService>));
            Assert.IsNotNull(raw);

            var asEnumerable = raw as IEnumerable<IService>;
            Assert.IsNotNull(asEnumerable);

            var list = asEnumerable.ToList();
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void UsesEnumerable_ConstructorInjection_ResolvesAll()
        {
            var services = new ServiceCollection();
            services.AddTransient<IServiceGet, ServiceAGet>();
            services.AddTransient<IServiceGet, ServiceBGet>();
            services.AddTransient<UsesEnumerable>();
            var provider = services.Build();

            var consumer = provider.GetService<UsesEnumerable>();

            Assert.AreEqual(2, consumer.Services.Count());
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, consumer.Services.Select(s => s.Get()).ToArray());
        }

        [TestMethod]
        public void UsesMixed_ConstructorInjection_ResolvesBothSingleAndMultiple()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IServiceGet, ServiceCGet>();
            services.AddTransient<IServiceGet, ServiceAGet>();
            services.AddTransient<IServiceGet, ServiceBGet>();
            services.AddTransient<UsesMixed>();
            var provider = services.Build();

            var instance = provider.GetService<UsesMixed>();

            Assert.IsInstanceOfType(instance.Single, typeof(ServiceCGet));
            Assert.AreEqual(3, instance.All.Count());
        }

        [TestMethod]
        public void EnumerableRegistration_WithScopedAndTransient_ResolvesAll()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IServiceGet, ServiceAGet>();
            services.AddTransient<IServiceGet, ServiceBGet>();
            services.AddTransient<IServiceGet, ServiceCGet>();
            var provider = services.Build();

            var all = provider.GetServices<IServiceGet>();

            Assert.AreEqual(3, all.Count());
        }

        [TestMethod]
        public void IEnumerableResolution_RepeatedCalls_DoNotCacheTransients()
        {
            var services = new ServiceCollection();
            services.AddTransient<IServiceGet, ServiceAGet>();
            services.AddTransient<IServiceGet, ServiceBGet>();
            var provider = services.Build();

            var first = provider.GetServices<IServiceGet>().ToList();
            var second = provider.GetServices<IServiceGet>().ToList();

            Assert.AreEqual(2, first.Count);
            Assert.AreEqual(2, second.Count);
            Assert.IsFalse(ReferenceEquals(first[0], second[0]));
            Assert.IsFalse(ReferenceEquals(first[1], second[1]));
        }

        [TestMethod]
        public void OptionsAndEnumerableResolution_CanCoexist()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptionsCount>(o => o.Count = 5);
            services.AddSingleton<IServiceGet, ServiceAGet>();
            services.AddSingleton<IServiceGet, ServiceBGet>();
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptionsCount>>();
            var allServices = provider.GetServices<IServiceGet>().ToList();

            Assert.AreEqual(5, options.Value.Count);
            Assert.AreEqual(2, allServices.Count);
        }

        [TestMethod]
        public void EmptyEnumerableResolution_ReturnsEmptyArray()
        {
            var services = new ServiceCollection();
            var provider = services.Build();

            var result = provider.GetServices<IServiceGet>();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void EnumerableInjection_WithMultipleInterfaces_WorksCorrectly()
        {
            var services = new ServiceCollection();
            services.AddTransient<IServiceGet, ServiceAGet>();
            services.AddTransient<IServiceGet, ServiceBGet>();
            services.AddTransient<IMarker, MarkerA>();
            services.AddTransient<IMarker, MarkerB>();
            services.AddTransient<ConsumesMultipleEnumerables>();
            var provider = services.Build();

            var consumer = provider.GetService<ConsumesMultipleEnumerables>();

            Assert.AreEqual(2, consumer.Services.Count());
            Assert.AreEqual(2, consumer.Markers.Count());
        }

        [TestMethod]
        public void OptionsRegisteredTwiceWithConfigure_CombinesConfigurators()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptionsCount>();
            services.Configure<MyOptionsCount>(o => o.Count += 10);
            services.Configure<MyOptionsCount>(o => o.Count *= 2);
            var provider = services.Build();

            var result = provider.GetService<IOptions<MyOptionsCount>>();

            Assert.AreEqual(20, result.Value.Count); // (0 + 10) * 2
        }

        [TestMethod]
        public void TransientService_InEnumerable_GetsDifferentInstancesEachTime()
        {
            var services = new ServiceCollection();
            services.AddTransient<IServiceGet, ServiceAGet>();
            services.AddTransient<IServiceGet, ServiceBGet>();
            services.AddTransient<UsesEnumerable>();
            var provider = services.Build();

            var first = provider.GetService<UsesEnumerable>().Services.ToList();
            var second = provider.GetService<UsesEnumerable>().Services.ToList();

            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreNotSame(first[i], second[i]);
            }
        }

        public interface IMarker { string Label(); }
        public class MarkerA : IMarker { public string Label() => "A"; }
        public class MarkerB : IMarker { public string Label() => "B"; }

        public class ConsumesMultipleEnumerables
        {
            public IEnumerable<IServiceGet> Services { get; }
            public IEnumerable<IMarker> Markers { get; }

            public ConsumesMultipleEnumerables(IEnumerable<IServiceGet> services, IEnumerable<IMarker> markers)
            {
                Services = services;
                Markers = markers;
            }
        }


        #region Test Interfaces and Classes

        public interface ITestService { }
        public class TestService : ITestService
        {
            public TestService() { }
        }
        public class AnotherTestService : ITestService { }

        public class MyOptions
        {
            public string Name { get; set; } = "default";
        }

        public interface IDependency { }
        public class Dependency : IDependency { }

        public class MultiConstructorClass
        {
            public IDependency Dependency { get; }
            public string Source { get; }

            public MultiConstructorClass(string manualParam)
            {
                Source = "manual";
            }

            public MultiConstructorClass(IDependency dependency)
            {
                Dependency = dependency;
                Source = "resolved";
            }
        }

        public class NullableArgumentClass
        {
            public string Text { get; }
            public NullableArgumentClass(string text)
            {
                Text = text;
            }
        }

        public interface IService { }
        public class ServiceImpl1 : IService { }
        public class ServiceImpl2 : IService { }
        public class ServiceA : IService { }
        public class ServiceB : IService { }
        public class DependentService
        {
            public IService Service { get; }
            public DependentService(IService service) => Service = service;
        }
        public interface IServiceGet { string Get(); }
        public class ServiceAGet : IServiceGet { public string Get() => "A"; }
        public class ServiceBGet : IServiceGet { public string Get() => "B"; }
        public class ServiceCGet : IServiceGet { public string Get() => "C"; }

        public class UsesEnumerable
        {
            public IEnumerable<IServiceGet> Services { get; }
            public UsesEnumerable(IEnumerable<IServiceGet> services) => Services = services;
        }

        public class UsesMixed
        {
            public IServiceGet Single { get; }
            public IEnumerable<IServiceGet> All { get; }
            public UsesMixed(IServiceGet single, IEnumerable<IServiceGet> all)
            {
                Single = single;
                All = all;
            }
        }
        public class MyOptionsCount { public int Count { get; set; } }

        #endregion
    }
}
