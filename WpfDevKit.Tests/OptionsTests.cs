using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit;
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

        private class OtherOptions
        {
            public string Name { get; set; }
        }

        public class ComplexOptions
        {
            public int Value { get; set; }
            public string Message { get; set; } = "default";
        }

        private class NoDefaultCtorOptions
        {
            public string Name { get; set; }
            public NoDefaultCtorOptions(string name) => Name = name;
        }

        [TestMethod]
        public void AddOptions_WithoutConfigure_ProvidesDefaultInstance()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            var provider = services.Build();

            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.IsNotNull(options);
            Assert.AreEqual(0, options.Value.Value);
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
            services.Configure<MyOptions>(o => o.Value = 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Configure_WithoutAddOptions_Throws()
        {
            var services = new ServiceCollection();
            services.Configure<MyOptions>(o => o.Value = 1);
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
            Assert.AreEqual(3, options.Value.Value);
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddOptions_CalledTwice_ThrowsException()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            services.AddOptions<MyOptions>();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddOptions_AndThenFactory_ThrowsException()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            services.AddOptions<MyOptions>(_ => new MyOptions());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddOptions_WithFactoryThenConfigure_ThrowsException()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>(_ => new MyOptions());
            services.Configure<MyOptions>(o => o.Value = 10);
        }

        [TestMethod]
        public void AddOptions_ThenConfigure_MultipleCalls_AppendsAll()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            services.Configure<MyOptions>(o => o.Value += 2);
            services.Configure<MyOptions>(o => o.Value += 3);

            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual(5, options.Value.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Configure_AfterBuild_Throws()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            var provider = services.Build();

            services.Configure<MyOptions>(o => o.Value = 99);
        }

        [TestMethod]
        public void AddOptions_MultipleConfigureOrders_RespectsOrder()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>();
            services.Configure<MyOptions>(o => o.Value = 5);
            services.Configure<MyOptions>(o => o.Value *= 10);

            var provider = services.Build();
            var options = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual(50, options.Value.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddOptions_WithFactoryAndConfigure_Throws()
        {
            var services = new ServiceCollection();
            services.AddOptions<MyOptions>(_ => new MyOptions { Value = 10 });
            services.Configure<MyOptions>(o => o.Value += 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddOptions_WithoutParameterlessCtor_ThrowsAtBuild()
        {
            var services = new ServiceCollection();

            // This compiles because we're registering manually (not using AddOptions<T>())
            services.AddSingleton<IOptions<NoDefaultCtorOptions>>();

            // Configuring will fail at runtime since there's no default constructor AND the options weren't added using AddOptions<TOptions>()
            services.Configure<NoDefaultCtorOptions>(o => o.Name = "Test");
        }

        [TestMethod]
        public void AddOptions_UsingFactory_BypassesParameterlessCtorRequirement()
        {
            var services = new ServiceCollection();
            services.AddOptions<NoDefaultCtorOptions>(_ => new NoDefaultCtorOptions("Hello"));

            var provider = services.Build();
            var result = provider.GetService<IOptions<NoDefaultCtorOptions>>();

            Assert.AreEqual("Hello", result.Value.Name);
        }

        [TestMethod]
        public void Configure_IsAppliedOnlyToMatchingOptionType()
        {
            var services = new ServiceCollection();
            services.AddOptions<ComplexOptions>();
            services.Configure<ComplexOptions>(o => o.Value = 999);
            services.AddOptions<MyOptions>();
            var provider = services.Build();

            var complex = provider.GetService<IOptions<ComplexOptions>>();
            var other = provider.GetService<IOptions<MyOptions>>();

            Assert.AreEqual(999, complex.Value.Value);
            Assert.AreEqual(0, other.Value.Value);
        }

        [TestMethod]
        public void Factory_CannotBeConfiguredLater()
        {
            var services = new ServiceCollection();
            services.AddOptions<ComplexOptions>(_ => new ComplexOptions { Message = "from factory" });

            Assert.ThrowsException<InvalidOperationException>(() =>
                services.Configure<ComplexOptions>(o => o.Message = "should fail"));
        }
    }
}
