// Unit tests for custom options registration and configuration in ServiceCollection

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Tests.DependencyInjection
{
    [TestClass]
    public class OptionsTests
    {
        private IServiceCollection services;

        [TestInitialize]
        public void Setup()
        {
            services = new ServiceCollection();
        }

        public class MyOptions
        {
            public string Name { get; set; } = "Default";
            public int Count { get; set; } = 1;
        }

        [TestMethod]
        public void AddOptions_RegistersDefaultOptions()
        {
            services.AddOptions<MyOptions>();
            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.IsNotNull(options);
            Assert.AreEqual("Default", options.Value.Name);
            Assert.AreEqual(1, options.Value.Count);
        }

        [TestMethod]
        public void Configure_ModifiesOptionsAfterAddOptions()
        {
            services.AddOptions<MyOptions>().Configure<MyOptions>(opt =>
            {
                opt.Name = "Updated";
                opt.Count = 42;
            });

            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual("Updated", options.Value.Name);
            Assert.AreEqual(42, options.Value.Count);
        }

        [TestMethod]
        public void AddOptions_WithFactory_RegistersCustomOptions()
        {
            services.AddOptions<MyOptions>(_ => new MyOptions
            {
                Name = "FromFactory",
                Count = 99
            });

            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual("FromFactory", options.Value.Name);
            Assert.AreEqual(99, options.Value.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Configure_ThrowsWhenOptionsRegisteredWithFactory()
        {
            services.AddOptions<MyOptions>(_ => new MyOptions());
            services.Configure<MyOptions>(opt => opt.Name = "ShouldFail");
        }

        [TestMethod]
        public void AddOptions_CanBeConfiguredWithMultipleActions()
        {
            services.AddOptions<MyOptions>()
                    .Configure<MyOptions>(opt => opt.Name = "Step1")
                    .Configure<MyOptions>(opt => opt.Count = 77);

            var provider = services.Build();
            var options = provider.GetRequiredService<IOptions<MyOptions>>();

            Assert.AreEqual("Step1", options.Value.Name);
            Assert.AreEqual(77, options.Value.Count);
        }
    }
}
