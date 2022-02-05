using System.CommandLine;
using System.CommandLine.Invocation;
using Kantaiko.Controllers.Introspection;
using Kantaiko.Properties;
using Kantaiko.Routing.Context;

namespace Kantaiko.CommandLine;

public class CommandLineContext : ContextBase
{
    internal CommandLineContext(InvocationContext invocationContext, EndpointInfo endpointInfo,
        IReadOnlyList<KeyValuePair<EndpointParameterInfo, Symbol>> parameters,
        IServiceProvider? serviceProvider = null,
        IReadOnlyPropertyCollection? properties = null,
        CancellationToken cancellationToken = default) :
        base(serviceProvider, properties, cancellationToken)
    {
        InvocationContext = invocationContext;
        EndpointInfo = endpointInfo;
        Parameters = parameters;
    }

    public InvocationContext InvocationContext { get; }
    public EndpointInfo EndpointInfo { get; }

    internal IReadOnlyList<KeyValuePair<EndpointParameterInfo, Symbol>> Parameters { get; }
}
