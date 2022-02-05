using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Kantaiko.Controllers.Introspection;
using Kantaiko.Controllers.Result;
using Microsoft.Extensions.DependencyInjection;

namespace Kantaiko.CommandLine.Internal;

internal class ControllerInvocationHandler : ICommandHandler
{
    private readonly EndpointInfo _endpointInfo;
    private readonly IReadOnlyList<KeyValuePair<EndpointParameterInfo, Symbol>> _parameters;

    public ControllerInvocationHandler(EndpointInfo endpointInfo,
        IReadOnlyList<KeyValuePair<EndpointParameterInfo, Symbol>> parameters)
    {
        _endpointInfo = endpointInfo;
        _parameters = parameters;
    }

    public async Task<int> InvokeAsync(InvocationContext invocationContext)
    {
        var serviceProvider = invocationContext.BindingContext.GetRequiredService<IServiceProvider>();

        var handler = serviceProvider.GetRequiredService<CommandLineHandlerAccessor>().Handler;

        var context = new CommandLineContext(invocationContext, _endpointInfo, _parameters,
            serviceProvider, cancellationToken: invocationContext.GetCancellationToken());

        var result = await handler.Handle(context);

        return ApplyExecutionResult(result, invocationContext);
    }

    private static int ApplyExecutionResult(ControllerExecutionResult result, InvocationContext invocationContext)
    {
        if (!result.IsMatched)
        {
            return -1;
        }

        if (result.IsExited)
        {
            switch (result.ExitReason)
            {
                case ExceptionExitReason exceptionExitReason:
                    throw exceptionExitReason.Exception;
                case ErrorExitReason errorExitReason:
                    invocationContext.Console.Error.WriteLine(errorExitReason.ErrorMessage!);
                    break;
            }

            return -1;
        }

        if (!result.HasReturnValue)
        {
            return 0;
        }

        switch (result.ReturnValue)
        {
            case int returnCode:
                return returnCode;
            case string content:
                invocationContext.Console.WriteLine(content);
                break;
        }

        return 0;
    }
}
