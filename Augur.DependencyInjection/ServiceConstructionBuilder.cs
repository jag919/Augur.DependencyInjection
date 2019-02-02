using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Augur.DependencyInjection
{
    public abstract class ServiceConstructionBuilder
    {
        internal abstract IEnumerable<ServiceDescriptor> BuildServiceDescriptors(ServiceLifetime lifetime);
    }
}
