using Kantaiko.CommandLine.Properties;
using Kantaiko.Controllers.Introspection.Factory.Attributes;
using Kantaiko.Controllers.Introspection.Factory.Context;
using Kantaiko.Properties.Immutable;

namespace Kantaiko.CommandLine;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute, IEndpointPropertyProvider
{
    private readonly string? _commandName;

    public CommandAttribute() { }

    public CommandAttribute(string? commandName)
    {
        _commandName = commandName;
    }

    public string? Description { get; init; }

    public IImmutablePropertyCollection UpdateEndpointProperties(EndpointFactoryContext context)
    {
        return context.Endpoint.Properties.Set(new CommandEndpointProperties(_commandName, Description));
    }
}
