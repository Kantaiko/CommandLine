using Kantaiko.Routing.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Kantaiko.CommandLine.Internal;

internal class ServiceHandlerFactory : IHandlerFactory
{
    public object CreateHandler(Type handlerType, IServiceProvider serviceProvider)
    {
        return ActivatorUtilities.CreateInstance(serviceProvider, handlerType);
    }
}
