using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Augur.DependencyInjection
{
    internal class WhereConstructorMap<TArgument, TService> : ConstructorMap<TArgument, TService>
      where TService : TArgument
    {
        public WhereConstructorMap(Func<TService, bool> selector) => Selector = selector;

        public Func<TService, bool> Selector { get; private set; }

        public override object GetService(IServiceProvider serviceProvider) => serviceProvider.GetServices<TService>().LastOrDefault(Selector);
    }
}
