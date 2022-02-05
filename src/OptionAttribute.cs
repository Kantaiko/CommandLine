using Kantaiko.CommandLine.Properties;
using Kantaiko.Controllers.Introspection.Factory.Attributes;
using Kantaiko.Controllers.Introspection.Factory.Context;
using Kantaiko.Properties.Immutable;

namespace Kantaiko.CommandLine;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class OptionAttribute : Attribute, IParameterPropertyProvider, IParameterCustomizationProvider
{
    private readonly string? _name;
    private readonly IReadOnlyList<string>? _aliases;

    public OptionAttribute() { }

    public OptionAttribute(string? name = null, params string[] aliases)
    {
        _name = name;
        _aliases = aliases;
    }

    public string? Description { get; init; }
    public bool IsOptional { get; init; }

    public IImmutablePropertyCollection UpdateParameterProperties(ParameterFactoryContext context)
    {
        return context.Parameter.Properties.Set(new CommandParameterProperties(true, _name, _aliases, Description));
    }

    public (string Name, bool IsOptional) GetParameterCustomization(ParameterFactoryContext context)
    {
        return (_name ?? context.Parameter.Name, context.Parameter.IsOptional || IsOptional);
    }
}
