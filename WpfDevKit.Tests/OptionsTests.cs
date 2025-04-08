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
    }
}
