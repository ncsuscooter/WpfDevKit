using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Factory;

namespace WpfDevKit.Tests.DependencyInjection
{
    [TestClass]
    public class ServiceProviderTests
    {
        private IServiceCollection services;

        [TestInitialize]
        public void Setup() => services = new ServiceCollection();

        [TestMethod]
        public void Singleton_ResolvesSameInstance()
        {
            services.AddSingleton<ITestService, TestService>();
            var provider = services.Build();

            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void Transient_ResolvesDifferentInstances()
        {
            services.AddTransient<ITestService, TestService>();
            var provider = services.Build();

            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void Factory_SingletonAndTransient_BehavesCorrectly()
        {
            services.AddSingleton<ISingletonService>(_ => new SingletonService());
            services.AddTransient<ITransientService>(_ => new TransientService());
            var provider = services.Build();

            var singleton1 = provider.GetService<ISingletonService>();
            var singleton2 = provider.GetService<ISingletonService>();
            Assert.AreSame(singleton1, singleton2);

            var transient1 = provider.GetService<ITransientService>();
            var transient2 = provider.GetService<ITransientService>();
            Assert.AreNotSame(transient1, transient2);
        }

        [TestMethod]
        public void Build_LogsWarning_WhenDuplicateServiceTypesAreRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<ITestService, TestService2>();

            var stringBuilder = new System.Text.StringBuilder();
            var writer = new StringWriter(stringBuilder);
            var listener = new TextWriterTraceListener(writer);
            Debug.Listeners.Clear(); // Clear default listeners (important in test context)
            Debug.Listeners.Add(listener);
            Debug.AutoFlush = true;

            // Act
            services.Build();

            // Read output
            var output = stringBuilder.ToString();

            // Cleanup
            Debug.Listeners.Remove(listener);

            // Assert
            StringAssert.Contains(output, "registered multiple times");
            StringAssert.Contains(output, typeof(ITestService).FullName);
        }

        [TestMethod]
        public void Options_RegistrationAndConfiguration_Works()
        {
            services.AddOptions<MyOptions>();
            services.Configure<MyOptions>(o => o.Name = "configured");
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();
            Assert.AreEqual("configured", options.Value.Name);
        }

        [TestMethod]
        public void EnumerableResolution_ResolvesAllRegistered()
        {
            services.AddTransient<ITestService, TestService>();
            services.AddTransient<ITestService, TestService2>();
            var provider = services.Build();

            var all = provider.GetServices<ITestService>().ToList();
            Assert.AreEqual(2, all.Count);
        }

        [TestMethod]
        public void MissingServices_ReturnsNullOrThrows()
        {
            var provider = services.Build();

            Assert.IsNull(provider.GetService<ITestService>());
            Assert.ThrowsException<InvalidOperationException>(() => provider.GetRequiredService<ITestService>());
        }

        [TestMethod]
        public void SelfRegistered_And_DependencyInjection_Works()
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddTransient<DependentService>();
            var provider = services.Build();

            var dependent = provider.GetService<DependentService>();
            Assert.IsNotNull(dependent);
            Assert.IsNotNull(dependent.Service);
        }

        [TestMethod]
        public void ResolvableProperty_IsInjected()
        {
            services.AddSingleton<InjectedDep>();
            services.AddTransient<NeedsResolvableProperty>();
            var provider = services.Build();

            var instance = provider.GetService<NeedsResolvableProperty>();
            Assert.IsNotNull(instance.Dep);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CircularDependency_ThrowsException()
        {
            services.AddTransient<A>();
            services.AddTransient<B>();
            var provider = services.Build();

            provider.GetService<A>();
        }

        public interface ITestService { }
        public class TestService : ITestService { }
        public class TestService2 : ITestService { }

        public interface ISingletonService { }
        public class SingletonService : ISingletonService { }

        public interface ITransientService { }
        public class TransientService : ITransientService { }

        public class MyOptions { public string Name { get; set; } = "default"; }

        public class DependentService
        {
            public ITestService Service { get; }
            public DependentService(ITestService service) => Service = service;
        }

        public class InjectedDep { }
        public class NeedsResolvableProperty
        {
            [Resolvable]
            public InjectedDep Dep { get; set; }
        }

        public class A { public A(B b) { } }
        public class B { public B(A a) { } }
    }
}
