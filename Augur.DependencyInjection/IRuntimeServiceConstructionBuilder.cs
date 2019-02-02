using System;

namespace Augur.DependencyInjection
{
    /// <summary>
    /// Defines methods that can be accessed with just <see cref="Type"/> parameters
    /// </summary>
    public interface IRuntimeServiceConstructionBuilder
    {
        IRuntimeServiceConstructionBuilder ConstructWith(Type argumentType, Type serviceType);

        IRuntimeServiceConstructionBuilder ConstructWith(object value);

        IRuntimeServiceConstructionBuilder AlsoAddAs(Type assignableType);

        IRuntimeServiceConstructionBuilder AlsoAddBaseTypes();

        IRuntimeServiceConstructionBuilder AlsoAddInterfaces();

        IRuntimeServiceConstructionBuilder OnCreation(Action<object> onCreation);
    }
}
