// Test project scaffold for your custom DI container (MSTest version)

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Tests.DI
{
    [TestClass]
    public class ServiceCollectionTests
    {
        [TestMethod]
        public void AddSingleton_ResolvesSameInstance()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();

            var provider = services.Build();
            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void AddTransient_ResolvesNewInstanceEachTime()
        {
            var services = new ServiceCollection();
            services.AddTransient<ITestService, TestService>();

            var provider = services.Build();
            var a = provider.GetService<ITestService>();
            var b = provider.GetService<ITestService>();

            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void CanResolve_DependencyGraph()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<IConsumer, Consumer>();

            var provider = services.Build();
            var consumer = provider.GetService<IConsumer>();

            Assert.IsNotNull(consumer);
            Assert.IsNotNull(consumer.TestService);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowsOnMissingDependency()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConsumer, Consumer>();

            var provider = services.Build();
            provider.GetService<IConsumer>();
        }

        [TestMethod]
        public void SupportsDelegateFactories()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService>(sp => new TestService("custom"));

            var provider = services.Build();
            var instance = provider.GetService<ITestService>();

            Assert.AreEqual("custom", instance.Name);
        }
    }

    public interface ITestService
    {
        string Name { get; }
    }

    public class TestService : ITestService
    {
        public string Name { get; }

        public TestService() => Name = "default";
        public TestService(string name) => Name = name;
    }

    public interface IConsumer
    {
        ITestService TestService { get; }
    }

    public class Consumer : IConsumer
    {
        public ITestService TestService { get; }

        public Consumer(ITestService service) => TestService = service;
    }
}
