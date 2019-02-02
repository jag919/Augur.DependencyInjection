using System;

namespace Augur.DependencyInjection
{
    internal abstract class ConstructorMap<TArgument> : ConstructorMap
    {
        public override Type ArgumentType => typeof(TArgument);

        public override Type ServiceType => typeof(TArgument);
    }
}
