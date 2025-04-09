using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Tests.DependencyInjection
{
    [TestClass]
    public class OptionsTests
    {
        private class MyOptions
        {
            public int Value { get; set; }
        }

        [TestMethod]
        public void AddOptions_WithoutConfigure_ProvidesDefaultInstance()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.IsNotNull(options);
            Assert.AreEqual(0, options.Value.Value); // default int value
        }

        [TestMethod]
        public void AddOptions_WithConfigure_AppliesConfiguration()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>(o => o.Value = 42);
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.IsNotNull(options);
            Assert.AreEqual(42, options.Value.Value);
        }

        [TestMethod]
        public void AddOptions_WithFactory_ProvidesFactoryValue()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>(_ => new MyOptions { Value = 123 });
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.IsNotNull(options);
            Assert.AreEqual(123, options.Value.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Configure_AfterFactoryRegistration_Throws()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>(_ => new MyOptions());
            services.Configure<MyOptions>(o => o.Value = 1); // should throw
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Configure_WithoutAddOptions_Throws()
        {
            var services = new ServiceCollection();
            services.Configure<MyOptions>(o => o.Value = 1); // should throw
        }

        [TestMethod]
        public void MultipleConfigurations_AreAppliedInOrder()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            services.Configure<MyOptions>(o => o.Value += 1);
            services.Configure<MyOptions>(o => o.Value += 2);
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual(3, options.Value.Value); // 0 + 1 + 2
        }

        [TestMethod]
        public void AddOptions_CanBeCalledMultipleTimesForSameType()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            services.Configure<MyOptions>(o => o.Value = 100);
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();
            Assert.AreEqual(100, options.Value.Value);
        }

        private class Dependency { public int GetValue() => 42; }

        [TestMethod]
        public void AddOptions_WithFactory_UsesDependency()
        {
            var services = new ServiceCollection();
            services.AddSingleton<Dependency>();
            services.AddOptions<MyOptions>(p =>
            {
                var dep = p.GetService<Dependency>();
                return new MyOptions { Value = dep.GetValue() };
            });

            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual(42, options.Value.Value);
        }

        private class OtherOptions
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void AddOptions_MultipleTypes_WorkIndependently()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>(o => o.Value = 77);
            services.AddOptions<OtherOptions>(o => o.Name = "Test");

            var provider = services.Build();
            var my = provider.GetService<IOptions<MyOptions>>();
            var other = provider.GetService<IOptions<OtherOptions>>();

            Assert.AreEqual(77, my.Value.Value);
            Assert.AreEqual("Test", other.Value.Name);
        }

        [TestMethod]
        public void AddOptions_AfterConfigure_StillAppliesConfiguration()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>(o => o.Value = 5);
            services.AddOptions<MyOptions>();
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();
            Assert.AreEqual(5, options.Value.Value);
        }

        [TestMethod]
        public void AddOptions_CalledTwice_DoesNotRegisterDuplicate()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            services.AddOptions<MyOptions>();
            services.Configure<MyOptions>(o => o.Value = 7);
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();
            Assert.AreEqual(7, options.Value.Value);
        }
    }
}
