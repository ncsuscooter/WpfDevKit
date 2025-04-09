// Unit tests for ServiceCollection and ServiceProvider behavior

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Activation;
using System.Linq;

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

        public class DependentService
        {
            public IService Service { get; }
            public DependentService(IService service) => Service = service;
        }
    }
}
