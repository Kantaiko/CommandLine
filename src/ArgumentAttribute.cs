using Kantaiko.CommandLine.Properties;
using Kantaiko.Controllers.Introspection.Factory.Attributes;
using Kantaiko.Controllers.Introspection.Factory.Context;
using Kantaiko.Properties.Immutable;

namespace Kantaiko.CommandLine;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class ArgumentAttribute : Attribute, IParameterPropertyProvider, IParameterCustomizationProvider
{
    private readonly string? _name;

    public ArgumentAttribute() { }

    public ArgumentAttribute(string name)
    {
        _name = name;
    }

    public string? Description { get; init; }
    public bool IsOptional { get; init; }

    public IImmutablePropertyCollection UpdateParameterProperties(ParameterFactoryContext context)
    {
        return context.Parameter.Properties.Set(new CommandParameterProperties(false, _name, null, Description));
    }

    public (string Name, bool IsOptional) GetParameterCustomization(ParameterFactoryContext context)
    {
        return (_name ?? context.Parameter.Name, context.Parameter.IsOptional || IsOptional);
    }
}
