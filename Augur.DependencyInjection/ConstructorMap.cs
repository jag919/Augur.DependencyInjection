using System;

namespace Augur.DependencyInjection
{
    // I am not sure all the casting is necessary in the sub classes, may be
    // able to simplify this.
    internal abstract class ConstructorMap
    {
        public abstract Type ArgumentType { get; }

        public abstract Type ServiceType { get; }

        public abstract object GetService(IServiceProvider serviceProvider);

        internal static ConstructorMap Create(Type constructorMapType, Type argumentType, params object[] arguments)
        {
            constructorMapType.AssertIsSubclassOf(typeof(ConstructorMap));

            var closedType = constructorMapType.MakeGenericType(argumentType);
            var constructorMap = Activator.CreateInstance(closedType, arguments) as ConstructorMap;
            return constructorMap;
        }

        internal static ConstructorMap Create(Type constructorMapType, Type argumentType, Type serviceType, params object[] arguments)
        {
            constructorMapType.AssertIsSubclassOf(typeof(ConstructorMap));
            argumentType.AssertIsAssignableFrom(serviceType);

            var closedType = constructorMapType.MakeGenericType(argumentType, serviceType);
            var constructorMap = Activator.CreateInstance(closedType, arguments) as ConstructorMap;
            return constructorMap;
        }
    }
}
