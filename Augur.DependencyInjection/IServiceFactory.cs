using System;

namespace Augur.DependencyInjection
{
    public interface IServiceFactory
    {
        object Create(IServiceProvider serviceProvider);
    }
}
