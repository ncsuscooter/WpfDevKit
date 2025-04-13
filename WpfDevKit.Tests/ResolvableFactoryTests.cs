using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Factory;

namespace WpfDevKit.Tests.Activation
{
    [TestClass]
    public class ResolvableFactoryTests
    {
        private IServiceProvider provider;
        private ObjectFactory factory;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDependency, Dependency>();
            services.AddSingleton<AnotherDependency>();
            services.AddSingleton<InternalLogger>();
            provider = services.Build();
            factory = new ObjectFactory(provider);
        }

        [TestMethod]
        public void Create_WithParameterlessConstructor_ReturnsInstance()
        {
            var result = factory.Create<NoDependencyClass>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Create_WithConstructorInjection_ReturnsInstanceWithResolvedDependency()
        {
            var result = factory.Create<ConstructorInjectedClass>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Dependency);
        }

        [TestMethod]
        public void Create_WithPropertyInjection_SetsResolvableProperties()
        {
            var result = factory.Create<PropertyInjectedClass>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Another);
        }

        [TestMethod]
        public void Create_WithArguments_OverridesResolvedServices()
        {
            var overrideDep = new Dependency();
            var result = factory.Create<ConstructorInjectedClass>(overrideDep);
            Assert.AreSame(overrideDep, result.Dependency);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_WithUnresolvableConstructor_ThrowsException()
        {
            factory.Create<UnresolvableClass>();
        }

        [TestMethod]
        public void Create_WithMultipleConstructors_PrefersResolvableConstructor()
        {
            var result = factory.Create<MultiConstructorClass>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Dependency);
            Assert.AreEqual("resolved", result.Source);
        }

        [TestMethod]
        public void Create_WithMultipleConstructors_UsesManualParameterWhenDependencyMissing()
        {
            var services = new ServiceCollection();
            services.AddSingleton<InternalLogger>();
            var localProvider = services.Build();
            var localFactory = new ObjectFactory(localProvider);

            var result = localFactory.Create<MultiConstructorClass>("manual input");

            Assert.IsNotNull(result);
            Assert.AreEqual("manual", result.Source);
        }

        [TestMethod]
        public void Create_WithOptionalConstructorParameter_UsesDefaultValue()
        {
            var result = factory.Create<OptionalConstructorClass>();
            Assert.IsNotNull(result);
            Assert.AreEqual(99, result.Value);
        }

        [TestMethod]
        public void Create_WithOptionalConstructorParameter_UsesProvidedValue()
        {
            var result = factory.Create<OptionalConstructorClass>(123);
            Assert.IsNotNull(result);
            Assert.AreEqual(123, result.Value);
        }

        [TestMethod]
        public void Create_WithMixedParameterSources_ResolvesAllCorrectly()
        {
            var result = factory.Create<MixedConstructorClass>("manualValue");
            Assert.IsNotNull(result);
            Assert.AreEqual("manualValue", result.Manual);
            Assert.IsNotNull(result.Dependency);
            Assert.AreEqual(88, result.Optional);
        }

        [TestMethod]
        public void Create_WithUnregisteredResolvableProperty_LogsWarningButSucceeds()
        {
            var services = new ServiceCollection();
            services.AddSingleton<InternalLogger>();
            var localProvider = services.Build();
            var localFactory = new ObjectFactory(localProvider);

            var result = localFactory.Create<UnregisteredResolvablePropertyClass>();
            Assert.IsNotNull(result);
            Assert.IsNull(result.MissingDep);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_WithNoPublicConstructors_ThrowsException()
        {
            factory.Create<NoPublicConstructorClass>();
        }

        [TestMethod]
        public void Create_WithNullManualArgument_UsesNullSuccessfully()
        {
            var result = factory.Create<NullableArgumentClass>((string)null);
            Assert.IsNotNull(result);
            Assert.IsNull(result.Text);
        }

        [TestMethod]
        public void Create_MultipleInstances_CacheIsolationConfirmed()
        {
            var first = factory.Create<ConstructorInjectedClass>();
            var second = factory.Create<ConstructorInjectedClass>();
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreNotSame(first, second); // Transient behavior
        }

        [TestMethod]
        public void Create_PrivateResolvableProperty_IsInjected()
        {
            var result = factory.Create<PrivateResolvablePropertyClass>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.GetInternal());
        }

        [TestMethod]
        public void Create_PropertyWithPrivateSetter_IsInjected()
        {
            var result = factory.Create<PrivateSetterProperty>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Another);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_ValueTypeParameterNotOptionalOrProvided_Throws()
        {
            factory.Create<ValueTypeRequired>();
        }

        [TestMethod]
        public void Create_ArrayConstructor_AllowsNullOrEmpty()
        {
            var result = factory.Create<ArrayConstructor>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Create_NullableValueTypeConstructor_UsesDefault()
        {
            var result = factory.Create<NullableValueTypeClass>();
            Assert.IsNotNull(result);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public void Create_AmbiguousConstructor_UsesFirstMatched()
        {
            var result = factory.Create<AmbiguousConstructorClass>("one");
            Assert.IsNotNull(result);
            Assert.AreEqual("one", result.A);
            Assert.IsNull(result.B);
        }

        [TestMethod]
        public void Create_ConstructorWithUnresolvableAndOptionalParam_UsesOptional()
        {
            var result = factory.Create<UnresolvableWithOptionalClass>();
            Assert.IsNotNull(result);
            Assert.AreEqual("fallback", result.Value);
        }

        [TestMethod]
        public void Create_WithDifferentManualArgs_AvoidsConstructorCacheConflict()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDependency, Dependency>();
            var provider = services.Build();
            var factory = new ObjectFactory(provider);

            var first = factory.Create<MultiConstructorClass>("manual"); // manual param, should use string ctor
            var second = factory.Create<MultiConstructorClass>();        // dependency param, should use IDependency ctor

            Assert.AreEqual("manual", first.Source);
            Assert.AreEqual("resolved", second.Source);
        }

        public class NoDependencyClass { }

        public interface IDependency { string Name { get; } }
        public class Dependency : IDependency { public string Name => "Test"; }

        public class ConstructorInjectedClass
        {
            public IDependency Dependency { get; }
            public ConstructorInjectedClass(IDependency dependency) => Dependency = dependency;
        }

        public class AnotherDependency { public int Id => 42; }

        public class PropertyInjectedClass
        {
            [Resolvable]
            public AnotherDependency Another { get; set; }
        }

        public class UnresolvableClass
        {
            public UnresolvableClass(string something) { }
        }

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

        public class OptionalConstructorClass
        {
            public int Value { get; }
            public OptionalConstructorClass(int value = 99)
            {
                Value = value;
            }
        }

        public class MixedConstructorClass
        {
            public IDependency Dependency { get; }
            public string Manual { get; }
            public int Optional { get; }
            public MixedConstructorClass(IDependency dependency, string manual, int optional = 88)
            {
                Dependency = dependency;
                Manual = manual;
                Optional = optional;
            }
        }

        public class UnregisteredResolvablePropertyClass
        {
            [Resolvable]
            public MissingDependency MissingDep { get; set; }
        }

        public class MissingDependency { }

        public class NoPublicConstructorClass
        {
            private NoPublicConstructorClass() { }
        }

        public class NullableArgumentClass
        {
            public string Text { get; }
            public NullableArgumentClass(string text)
            {
                Text = text;
            }
        }

        public class PrivateResolvablePropertyClass
        {
            [Resolvable]
            private AnotherDependency InternalDep { get; set; }
            public AnotherDependency GetInternal() => InternalDep;
        }

        public class PrivateSetterProperty
        {
            [Resolvable]
            public AnotherDependency Another { get; private set; }
        }

        public class ValueTypeRequired
        {
            public int Number { get; }
            public ValueTypeRequired(int number) => Number = number;
        }

        public class ArrayConstructor
        {
            public string[] Values { get; }
            public ArrayConstructor(string[] values = null) => Values = values;
        }

        public class NullableValueTypeClass
        {
            public int? Value { get; }
            public NullableValueTypeClass(int? value = null) => Value = value;
        }

        public class AmbiguousConstructorClass
        {
            public string A { get; }
            public string B { get; }
            public AmbiguousConstructorClass(string a, string b = null)
            {
                A = a;
                B = b;
            }
        }

        public class UnresolvableWithOptionalClass
        {
            public string Value { get; }
            public UnresolvableWithOptionalClass(UnregisteredDependency missing = null, string value = "fallback")
            {
                Value = value;
            }
        }

        public class UnregisteredDependency { }
    }
}
