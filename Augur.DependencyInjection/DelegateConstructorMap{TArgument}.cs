using System;

namespace Augur.DependencyInjection
{
    internal class DelegateConstructorMap<TArgument> : ConstructorMap<TArgument>
    {
        public DelegateConstructorMap(Func<IServiceProvider, object> factory) => Factory = factory;

        public Func<IServiceProvider, object> Factory { get; private set; }

        public override object GetService(IServiceProvider serviceProvider) => Factory(serviceProvider);
    }
}
