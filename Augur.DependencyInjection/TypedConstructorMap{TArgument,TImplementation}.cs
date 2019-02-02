using System;

namespace Augur.DependencyInjection
{
    internal class TypedConstructorMap<TArgument, TImplementation> : ConstructorMap<TArgument, TImplementation>
      where TImplementation : TArgument
    {
        public override object GetService(IServiceProvider serviceProvider) => serviceProvider.GetService(ServiceType);
    }
}
