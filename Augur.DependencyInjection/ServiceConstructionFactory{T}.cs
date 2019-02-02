using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Augur.DependencyInjection
{
    public class ServiceConstructionFactory<T> : IServiceFactory
    {
        private readonly ConstructorMap[] _constructorMap;
        private readonly ObjectFactory _factory;
        private readonly Action<T> _onCreation;

        public ServiceConstructionFactory()
        {
            _constructorMap = Array.Empty<ConstructorMap>();
            _factory = ActivatorUtilities.CreateFactory(typeof(T), Array.Empty<Type>());
            _onCreation = null;
        }

        internal ServiceConstructionFactory(IEnumerable<ConstructorMap> constructorMap, Action<T> onCreation)
        {
            _constructorMap = constructorMap.ToArray();
            _factory = ActivatorUtilities.CreateFactory(typeof(T), _constructorMap.Select(map => map.ArgumentType).ToArray());
            _onCreation = onCreation;
        }

        public object Create(IServiceProvider serviceProvider)
        {
            var mapped = _constructorMap.Select(map => map.GetService(serviceProvider)).ToArray();
            var instance = _factory(serviceProvider, mapped);
            _onCreation?.Invoke((T)instance);

            return instance;
        }
    }
}
