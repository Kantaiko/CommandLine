using System.CommandLine.Parsing;
using Kantaiko.Controllers.Introspection;
using Kantaiko.Controllers.ParameterConversion.Text;
using Kantaiko.Controllers.ParameterConversion.Text.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace Kantaiko.CommandLine.Internal;

internal class CommandParameterConverter
{
    private readonly EndpointParameterInfo _parameterInfo;
    private readonly IServiceProvider _serviceProvider;

    private object? _result;

    public CommandParameterConverter(EndpointParameterInfo parameterInfo, IServiceProvider serviceProvider)
    {
        _parameterInfo = parameterInfo;
        _serviceProvider = serviceProvider;

        if (TextConversionParameterProperties.Of(parameterInfo) is null)
        {
            throw new InvalidOperationException();
        }
    }

    public string? Validate(ArgumentResult result)
    {
        var parameters = new Dictionary<string, string>
        {
            [result.Argument.Name] = result.Tokens[0].Value
        };

        return Validate(new TextParameterConversionContext(parameters, _parameterInfo));
    }

    public string? Validate(OptionResult result)
    {
        var parameters = new Dictionary<string, string>
        {
            [result.Option.Name] = result.Tokens[0].Value
        };

        return Validate(new TextParameterConversionContext(parameters, _parameterInfo));
    }

    private string? Validate(TextParameterConversionContext context)
    {
        var converter = CreateConverter();

        var validationResult = converter.Validate(context);

        if (!validationResult.IsValid)
        {
            return validationResult.ErrorMessage;
        }

        var resolutionResult = converter.ResolveAsync(context).GetAwaiter().GetResult();

        if (!resolutionResult.Success)
        {
            return resolutionResult.ErrorMessage;
        }

        _result = resolutionResult.Value;
        return null;
    }

    private ITextParameterConverter CreateConverter()
    {
        if (TextConversionParameterProperties.Of(_parameterInfo) is not { } properties)
        {
            throw new InvalidOperationException();
        }

        if (properties.ConverterFactory is not null)
        {
            return properties.ConverterFactory(_serviceProvider);
        }

        if (properties.ConverterType is not null)
        {
            return (ITextParameterConverter) ActivatorUtilities.CreateInstance(
                _serviceProvider, properties.ConverterType);
        }

        throw new InvalidOperationException();
    }

    public object Convert(ArgumentResult result) => _result!;
}
