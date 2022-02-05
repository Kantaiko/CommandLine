using Kantaiko.CommandLine.Properties;
using Kantaiko.Controllers.Introspection.Factory.Attributes;
using Kantaiko.Controllers.Introspection.Factory.Context;
using Kantaiko.Properties.Immutable;

namespace Kantaiko.CommandLine;

[AttributeUsage(AttributeTargets.Class)]
public class CommandGroupAttribute : Attribute, IControllerPropertyProvider
{
    private readonly string _name;

    public CommandGroupAttribute(string name)
    {
        _name = name;
    }

    public string? Description { get; init; }
    public Type? Parent { get; init; }

    public IImmutablePropertyCollection UpdateControllerProperties(ControllerFactoryContext context)
    {
        return context.Controller.Properties.Set(new CommandGroupControllerProperties(_name, Description, Parent));
    }
}
