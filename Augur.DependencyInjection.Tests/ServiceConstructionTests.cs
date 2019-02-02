using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Augur.DependencyInjection.Tests
{

    [TestClass]
    public class ServiceConstructionTests
    {
        [TestMethod]
        public void CanResolveInstanceByType()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddTransient<DependencyA>()
              .AddTransient<DependencyB>()
              .AddTransient<DependencyC>()
              .AddTransient<ISimple, Simple>()
              .AddTransientWithOptions<MyService>(o => o.ConstructWith<IDependency, DependencyB>())
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var myService = provider.GetService<MyService>();
            Assert.AreEqual(myService.ToString(), "Simple, B");
        }

        [TestMethod]
        public void CanResolveInstanceByTypeFromConfig()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddTransient<DependencyA>()
              .AddTransient<DependencyB>()
              .AddTransient<DependencyC>()
              .AddTransient<ISimple, Simple>()
              .AddWithOptions(typeof(MyService), ServiceLifetime.Transient, o => o.ConstructWith(typeof(IDependency), typeof(DependencyB)))
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var myService = provider.GetService<MyService>();
            Assert.AreEqual(myService.ToString(), "Simple, B");
        }

        /* WorkInProgress
        [TestMethod]
        public void CanResolveInstanceByName()
        {
          var services = new ServiceCollection();

          var provider = services
            .AddTransientWithOptions<IDependency, DependencyA>(o => o.Named("a"))
            .AddTransientWithOptions<IDependency, DependencyB>(o => o.Named("b"))
            .AddTransientWithOptions<IDependency, DependencyC>(o => o.Named("c"))
            .AddTransient<ISimple, Simple>()
            .AddTransientWithOptions<MyService>(o => o.ConstructWith<IDependency>("b"))
            .WhatDoIHave()
            .BuildServiceProvider();

          // When
          var myService = provider.GetService<MyService>();
          Assert.AreEqual(myService.ToString(), "Simple, B");
        }

        [TestMethod]
        public void CanResolveInstanceByNameFromConfig()
        {
          var services = new ServiceCollection();

          var provider = services
            .AddTransientWithOptions<IDependency, DependencyA>(o => o.Named("a"))
            .AddTransientWithOptions<IDependency, DependencyB>(o => o.Named("b"))
            .AddTransientWithOptions<IDependency, DependencyC>(o => o.Named("c"))
            .AddTransient<ISimple, Simple>()
            .AddTransientWithOptions<MyService>(o => o.ConstructWith("b", typeof(IDependency)))
            .WhatDoIHave()
            .BuildServiceProvider();

          // When
          var myService = provider.GetService<MyService>();
          Assert.AreEqual(myService.ToString(), "Simple, B");
        }
        */

        [TestMethod]
        public void CanResolveInstanceBySelector()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddTransient<IDependency, DependencyA>()
              .AddTransient<IDependency, DependencyB>()
              .AddTransient<IDependency, DependencyC>()
              .AddTransient<ISimple, Simple>()
              .AddTransientWithOptions<MyService>(o => o.ConstructWith<IDependency>(i => i.Name.EndsWith("B", StringComparison.InvariantCultureIgnoreCase)))
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var myService = provider.GetService<MyService>();
            Assert.AreEqual(myService.ToString(), "Simple, B");
        }

        [TestMethod]
        public void CanResolveInstanceWithValue()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddTransientWithOptions<MyEcho>(o => o.ConstructWith(5))
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var myService = provider.GetService<MyEcho>();
            Assert.AreEqual(myService.ToString(), "5");
        }

        [TestMethod]
        public void CanResolveInstanceWithValueFromConfig()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddWithOptions(typeof(MyEcho), ServiceLifetime.Transient, o => o.ConstructWith(Convert.ChangeType("5", typeof(int), CultureInfo.InvariantCulture)))
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var myService = provider.GetService<MyEcho>();
            Assert.AreEqual(myService.ToString(), "5");
        }

        [TestMethod]
        public void CanResolveInstanceWithValues()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddTransient<ISimple, Simple>()
              .AddTransientWithOptions<ConstructorArgTest>(o => o.ConstructWith("one").ConstructWith("two"))
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var myService = provider.GetService<ConstructorArgTest>();
            Assert.AreEqual(myService.ToString(), "Simple one two");
        }

        [TestMethod]
        public void CanResolveInstanceByAliases()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddSingletonWithOptions<DependencyA>(o => o.AlsoAddInterfaces())
              .AddSingletonWithOptions<DependencyB>(o => o.AlsoAddInterfaces())
              .AddSingletonWithOptions<DependencyC>(o => o.AlsoAddInterfaces().AlsoAddBaseTypes())
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var dependencyC = provider.GetService<DependencyC>();
            var defaultIDependency = provider.GetService<IDependency>();
            var defaultDependency = provider.GetService<Dependency>();

            Assert.AreEqual(dependencyC.Uid, defaultIDependency.Uid);
            Assert.AreEqual(dependencyC.Uid, defaultDependency.Uid);
            Assert.AreEqual(provider.GetServices<IDependency>().Count(), 3);
            Assert.AreEqual(provider.GetServices<Dependency>().Count(), 1);
        }

        [TestMethod]
        public void CanResolveInstanceByExplicitAliases()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddSingletonWithOptions<DependencyC>(o => o.AlsoAddAs<Dependency>())
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var dependencyC = provider.GetService<DependencyC>();
            var defaultIDependency = provider.GetService<IDependency>();
            var defaultDependency = provider.GetService<Dependency>();

            Assert.IsNull(defaultIDependency);
            Assert.IsNotNull(defaultDependency);
            Assert.AreEqual(dependencyC.Uid, defaultDependency.Uid);
        }

        [TestMethod]
        public void CanRaiseTheOnCreationDelegate()
        {
            var services = new ServiceCollection();
            DependencyA created = null;
            int count = 0;

            var provider = services
              .AddSingletonWithOptions<DependencyA>(o => o.AlsoAddInterfaces().OnCreation(i =>
              {
                  count++;
                  created = i;
              }))
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var defaultIDependency = provider.GetService<IDependency>();

            // Check singleton behavior is correct
            Assert.IsNotNull(defaultIDependency);
            Assert.AreEqual(defaultIDependency.Uid, provider.GetService<IDependency>()?.Uid);
            Assert.AreEqual(defaultIDependency.Uid, provider.GetService<DependencyA>()?.Uid);

            // Check creation was invoked and invoked only once
            Assert.IsNotNull(created);
            Assert.AreEqual(created.Uid, defaultIDependency.Uid);
            Assert.AreEqual(count, 1);
        }

        [TestMethod]
        public void CanResolveDecoratorsByType()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddTransient<IDependency, DependencyB>()
              .ReplaceWithDecorator<IDependency, DependencyDecorator>()
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var depedency = provider.GetService<IDependency>();
            Assert.IsInstanceOfType(depedency, typeof(DependencyDecorator));
            Assert.AreEqual(depedency.Name, "b");
        }

        [TestMethod]
        public void CanResolveDecoratorsByInstance()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddSingleton<IDependency>(new DependencyB())
              .ReplaceWithDecorator<IDependency, DependencyDecorator>()
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var depedency = provider.GetService<IDependency>();
            Assert.IsInstanceOfType(depedency, typeof(DependencyDecorator));
            Assert.AreEqual(depedency.Name, "b");
        }

        [TestMethod]
        public void CanResolveDecoratorsByFactory()
        {
            var services = new ServiceCollection();

            var provider = services
              .AddTransient<IDependency>(sp => new DependencyB())
              .ReplaceWithDecorator<IDependency, DependencyDecorator>()
              .WriteDiagnostics()
              .BuildServiceProvider();

            // When
            var depedency = provider.GetService<IDependency>();
            Assert.IsInstanceOfType(depedency, typeof(DependencyDecorator));
            Assert.AreEqual(depedency.Name, "b");
        }

        /*************************************************************
        ** TEST CLASSES
        *************************************************************/

#pragma warning disable CA1034
#pragma warning disable SA1201 // Elements must appear in the correct order
        public interface IDependency
#pragma warning restore SA1201 // Elements must appear in the correct order
        {
            Guid Uid { get; }

            string Name { get; }
        }

#pragma warning disable SA1201 // Elements must appear in the correct order
        public interface ISimple
#pragma warning restore SA1201 // Elements must appear in the correct order
        {
            string Name { get; }
        }

        public abstract class Dependency : IDependency
        {
            protected Dependency() => Uid = Guid.NewGuid();

            public Guid Uid { get; }

            public abstract string Name { get; }
        }

        public class DependencyA : Dependency
        {
            public override string Name => "A";
        }

        public class DependencyB : Dependency
        {
            public override string Name => "B";
        }

        public class DependencyC : Dependency
        {
            public override string Name => "C";
        }

        public class DependencyDecorator : IDependency
        {
            private readonly IDependency _inner;

            public DependencyDecorator(IDependency inner)
            {
                _inner = inner;
            }

            public Guid Uid => _inner.Uid;

            public string Name => $"{_inner.Name.ToLower(CultureInfo.InvariantCulture)}";
        }

        public class Simple : ISimple
        {
            public string Name => "Simple";
        }

        public class MyService
        {
            private readonly IDependency _dependency;
            private readonly ISimple _simple;

            public MyService(IDependency dependency, ISimple simple)
            {
                _dependency = dependency;
                _simple = simple;
            }

            public override string ToString()
            {
                return $"{_simple.Name}, {_dependency.Name}";
            }
        }

        public class MyEcho
        {
            private readonly int _number;

            public MyEcho(int number) => _number = number;

            public override string ToString() => $"{_number}";
        }

        public class ConstructorArgTest
        {
            public ConstructorArgTest(ISimple simple, string arg1, string arg2) => Text = $"{simple.Name} {arg1} {arg2}";

            public string Text { get; private set; }

            public override string ToString() => Text;
        }

#pragma warning restore CA1034
    }
}
