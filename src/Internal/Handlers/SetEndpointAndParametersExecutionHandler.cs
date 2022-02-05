using System.CommandLine;
using Kantaiko.Controllers.Execution;
using Kantaiko.Controllers.Execution.Handlers;
using Kantaiko.Controllers.Introspection;
using Kantaiko.Controllers.Result;

namespace Kantaiko.CommandLine.Internal.Handlers;

internal class SetEndpointAndParametersExecutionHandler : ControllerExecutionHandler<CommandLineContext>
{
    protected override Task<ControllerExecutionResult> HandleAsync(
        ControllerExecutionContext<CommandLineContext> context, NextAction next)
    {
        context.Endpoint = context.RequestContext.EndpointInfo;

        var parseResult = context.RequestContext.InvocationContext.BindingContext.ParseResult;
        var resolvedParameters = new Dictionary<EndpointParameterInfo, object?>();

        foreach (var (endpointInfo, symbol) in context.RequestContext.Parameters)
        {
            switch (symbol)
            {
                case IArgument argument:
                    resolvedParameters[endpointInfo] = parseResult.GetValueForArgument(argument);
                    continue;
                case Option option:
                    resolvedParameters[endpointInfo] = parseResult.GetValueForOption(option);
                    continue;
            }
        }

        context.ResolvedParameters = resolvedParameters;

        return next();
    }
}
