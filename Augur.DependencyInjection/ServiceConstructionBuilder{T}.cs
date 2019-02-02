using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Augur.DependencyInjection
{
    public class ServiceConstructionBuilder<T> : ServiceConstructionBuilder, IRuntimeServiceConstructionBuilder
    {
        // Options
        private readonly List<ConstructorMap> _constructorMap = new List<ConstructorMap>();
        private readonly List<Type> _aliases = new List<Type>();
        private Action<T> _onCreation;

        public ServiceConstructionBuilder<T> ConstructWith<TArgument, TService>()
          where TService : TArgument
        {
            _constructorMap.Add(new TypedConstructorMap<TArgument, TService>());
            return this;
        }

        /* WorkInProgress
        public ServiceConstructionBuilder<T> ConstructWith<TArgument>(string named)
        {
          _constructorMap.Add(new NamedConstructorMap<TArgument, TArgument>(named));
          return this;
        }

        public ServiceConstructionBuilder<T> ConstructWith<TArgument, TImplementation>(string named)
          where TImplementation : TArgument
        {
          _constructorMap.Add(new NamedConstructorMap<TArgument, TImplementation>(named));
          return this;
        }
        */

        public ServiceConstructionBuilder<T> ConstructWith<TArgument>(Func<TArgument, bool> selector)
        {
            _constructorMap.Add(new WhereConstructorMap<TArgument, TArgument>(selector));
            return this;
        }

        public ServiceConstructionBuilder<T> ConstructWith<TArgument, TService>(Func<TService, bool> selector)
          where TService : TArgument
        {
            _constructorMap.Add(new WhereConstructorMap<TArgument, TService>(selector));
            return this;
        }

        public ServiceConstructionBuilder<T> ConstructWith<TArgument, TService>(Func<IServiceProvider, object> factory)
          where TService : TArgument
        {
            _constructorMap.Add(new DelegateConstructorMap<TArgument>(factory));
            return this;
        }

        public ServiceConstructionBuilder<T> ConstructWith<TValue>(TValue value)
        {
            _constructorMap.Add(new ValueConstructorMap<TValue>(value));
            return this;
        }

        public ServiceConstructionBuilder<T> AlsoAddAs<TAssignable>()
        {
            typeof(TAssignable).AssertIsAssignableFrom(typeof(T));
            _aliases.Add(typeof(TAssignable));
            return this;
        }

        public ServiceConstructionBuilder<T> AlsoAddBaseTypes(Func<Type, bool> selector = null)
        {
            _aliases.AddRange(typeof(T).GetBaseTypes().Where(selector ?? TypeExtensions.IsNonSystemType));
            return this;
        }

        public ServiceConstructionBuilder<T> AlsoAddInterfaces(Func<Type, bool> selector = null)
        {
            _aliases.AddRange(typeof(T).GetInterfaces().Where(selector ?? TypeExtensions.IsNonSystemType));
            return this;
        }

        public ServiceConstructionBuilder<T> OnCreation(Action<T> onCreation)
        {
            _onCreation = onCreation;
            return this;
        }

        /*****************************************************************************************************************************
         ** This is an explicit interface used to access members of the builder without strong type information
        *****************************************************************************************************************************/

        IRuntimeServiceConstructionBuilder IRuntimeServiceConstructionBuilder.ConstructWith(Type argumentType, Type serviceType)
        {
            _constructorMap.Add(ConstructorMap.Create(typeof(TypedConstructorMap<,>), argumentType, serviceType));
            return this;
        }

        /* WorkInProgress
        public ServiceConstructionBuilder<T> ConstructWith(string named, Type argumentType, Type serviceType = null)
        {
          _constructorMap.Add(ConstructorMap.Create(typeof(NamedConstructorMap<,>), argumentType, serviceType ?? argumentType, named));
          return this;
        }
        */

        IRuntimeServiceConstructionBuilder IRuntimeServiceConstructionBuilder.ConstructWith(object value)
        {
            _constructorMap.Add(ConstructorMap.Create(typeof(ValueConstructorMap<>), value.GetType(), value));
            return this;
        }

        IRuntimeServiceConstructionBuilder IRuntimeServiceConstructionBuilder.AlsoAddAs(Type assignableType)
        {
            assignableType.AssertIsAssignableFrom(typeof(T));
            _aliases.Add(assignableType);
            return this;
        }

        IRuntimeServiceConstructionBuilder IRuntimeServiceConstructionBuilder.AlsoAddBaseTypes()
        {
            return AlsoAddBaseTypes(null);
        }

        IRuntimeServiceConstructionBuilder IRuntimeServiceConstructionBuilder.AlsoAddInterfaces()
        {
            return AlsoAddInterfaces(null);
        }

        IRuntimeServiceConstructionBuilder IRuntimeServiceConstructionBuilder.OnCreation(Action<object> onCreation)
        {
            _onCreation = t => onCreation(t);
            return this;
        }

        // Create the service descriptors defined by the options
        internal override IEnumerable<ServiceDescriptor> BuildServiceDescriptors(ServiceLifetime lifetime)
        {
            if (!_constructorMap.Any() && _onCreation == null)
            {
                // Short Circuit to a Standard Descriptor
                yield return new ServiceDescriptor(typeof(T), typeof(T), lifetime);
            }
            else
            {
                // If the lamda is not good enough to preserve state, I think we will do it in 2 passes:
                // _services.AddSingleton<ServiceConstructionFactory<T>>(factory);
                // _service.Add(new ServiceDescriptor(typeof(T), s => s.GetService<ServiceConstructionBuilder<T>>().Create(s), _lifetime));
                var factory = new ServiceConstructionFactory<T>(_constructorMap, _onCreation);
                yield return new ServiceDescriptor(typeof(T), factory.Create, lifetime);
            }

            // Register aliases that delegate construction to concrete type
            foreach (var aliasType in _aliases.Distinct())
            {
                // This is not perfect if multiple concretes register the same base type
                yield return new ServiceDescriptor(aliasType, GetService, lifetime);
            }
        }

        // Created as a separate method for readability in WhatDoIHave
        private static object GetService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<T>();
        }
    }

}
