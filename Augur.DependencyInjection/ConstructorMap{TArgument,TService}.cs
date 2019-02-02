using System;

namespace Augur.DependencyInjection
{
    internal abstract class ConstructorMap<TArgument, TService> : ConstructorMap
      where TService : TArgument
    {
        public override Type ArgumentType => typeof(TArgument);

        public override Type ServiceType => typeof(TService);
    }
}
