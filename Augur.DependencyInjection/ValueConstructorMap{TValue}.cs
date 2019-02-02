using System;

namespace Augur.DependencyInjection
{
    internal class ValueConstructorMap<TValue> : ConstructorMap<TValue>
    {
        public ValueConstructorMap(TValue value) => Value = value;

        public TValue Value { get; private set; }

        public override object GetService(IServiceProvider serviceProvider) => Value;
    }
}
