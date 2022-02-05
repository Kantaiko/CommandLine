using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Kantaiko.CommandLine.Properties;
using Kantaiko.Controllers.Introspection;
using Kantaiko.Controllers.ParameterConversion.Properties;

namespace Kantaiko.CommandLine.Internal;

internal class CommandBuilder
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<ControllerInfo, Command> _commandGroupCache = new();
    private readonly Dictionary<Command, EndpointInfo> _commandEndpointCache = new();

    private RootCommand? _rootCommand;

    public CommandBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public RootCommand CreateRootCommand(string executableName, string? description,
        IntrospectionInfo introspectionInfo)
    {
        _rootCommand = new RootCommand
        {
            Name = executableName,
            Description = description
        };

        foreach (var controller in introspectionInfo.Controllers)
        {
            if (TryGetCommandGroupCommand(controller, out _))
            {
                continue;
            }

            ProcessRootCommands(controller);
        }

        return _rootCommand;
    }

    private void ProcessRootCommands(ControllerInfo controllerInfo)
    {
        Debug.Assert(_rootCommand is not null);

        foreach (var endpoint in controllerInfo.Endpoints)
        {
            if (CommandEndpointProperties.Of(endpoint) is not
                { CommandName: var commandName, Description: var description })
            {
                continue;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (commandName is not null)
            {
                var command = new Command(commandName, description);
                ApplyCommandEndpoint(command, endpoint);

                _rootCommand.AddCommand(command);

                continue;
            }

            if (_commandEndpointCache.TryGetValue(_rootCommand, out var existingEndpointInfo))
            {
                throw new InvalidOperationException(
                    "Multiple root commands defined." +
                    $"Got at least \"{existingEndpointInfo.MethodInfo.Name}\" and \"{endpoint.MethodInfo.Name}\"");
            }

            _commandEndpointCache[_rootCommand] = endpoint;
            ApplyCommandEndpoint(_rootCommand, endpoint);

            if (string.IsNullOrEmpty(_rootCommand.Description))
            {
                _rootCommand.Description = description;
            }
        }
    }

    private bool TryGetCommandGroupCommand(ControllerInfo controllerInfo, [NotNullWhen(true)] out Command? command)
    {
        if (_commandGroupCache.TryGetValue(controllerInfo, out var existingCommand))
        {
            command = existingCommand;
            return true;
        }

        if (CommandGroupControllerProperties.Of(controllerInfo) is not var (groupName, description, parentType))
        {
            command = null;
            return false;
        }

        command = new Command(groupName, description);

        if (parentType is not null)
        {
            var parentController = controllerInfo.IntrospectionInfo!.Controllers
                .FirstOrDefault(x => x.Type == parentType);

            if (parentController is null)
            {
                throw new InvalidOperationException(
                    $"Parent type \"{parentType.FullName}\" is not a valid controller type");
            }

            if (!TryGetCommandGroupCommand(parentController, out var parentCommandInfo))
            {
                throw new InvalidOperationException(
                    $"Controller \"{controllerInfo.Type.FullName}\" is not a command group");
            }

            parentCommandInfo.AddCommand(command);
        }
        else
        {
            Debug.Assert(_rootCommand is not null);

            _rootCommand.AddCommand(command);
        }

        _commandGroupCache[controllerInfo] = command;
        ApplyCommandEndpoint(command, controllerInfo.Endpoints);

        return true;
    }

    private void ApplyCommandEndpoint(Command parent, IEnumerable<EndpointInfo> endpointInfos)
    {
        foreach (var endpointInfo in endpointInfos)
        {
            if (CommandEndpointProperties.Of(endpointInfo) is not var (commandName, description))
            {
                throw new InvalidOperationException();
            }

            if (commandName is not null)
            {
                var command = new Command(commandName, description);
                ApplyCommandEndpoint(command, endpointInfo);

                parent.AddCommand(command);
                continue;
            }

            if (_commandEndpointCache.TryGetValue(parent, out var existingEndpointInfo))
            {
                throw new InvalidOperationException(
                    $"Multiple root commands defined for group \"{parent.Name}\"." +
                    $"Got at least \"{endpointInfo.MethodInfo.Name}\" and \"{existingEndpointInfo.MethodInfo.Name}\"");
            }

            _commandEndpointCache[parent] = endpointInfo;
            ApplyCommandEndpoint(parent, endpointInfo);
        }
    }

    private void ApplyCommandEndpoint(Command command, EndpointInfo endpointInfo)
    {
        var parameters = ApplyParameters(command, endpointInfo);

        command.Handler = new ControllerInvocationHandler(endpointInfo, parameters);
    }

    private IReadOnlyList<KeyValuePair<EndpointParameterInfo, Symbol>> ApplyParameters(
        Command command, EndpointInfo endpointInfo)
    {
        var parameters = new List<KeyValuePair<EndpointParameterInfo, Symbol>>(endpointInfo.Parameters.Count);

        foreach (var parameterInfo in endpointInfo.Parameters)
        {
            if (ParameterServiceProperties.Of(parameterInfo) is not null)
            {
                continue;
            }

            var converter = new CommandParameterConverter(parameterInfo, _serviceProvider);

            if (TryApplyExplicitOption(parameterInfo, converter, out var option))
            {
                command.AddOption(option);
                parameters.Add(new KeyValuePair<EndpointParameterInfo, Symbol>(parameterInfo, option));

                continue;
            }

            if (TryApplyExplicitArgument(parameterInfo, converter, out var argument))
            {
                command.AddArgument(argument);
                parameters.Add(new KeyValuePair<EndpointParameterInfo, Symbol>(parameterInfo, argument));

                continue;
            }

            var explicitArgument = ApplyImplicitArgument(parameterInfo, converter);

            command.AddArgument(explicitArgument);
            parameters.Add(new KeyValuePair<EndpointParameterInfo, Symbol>(parameterInfo, explicitArgument));
        }

        return parameters;
    }

    private static bool TryApplyExplicitOption(EndpointParameterInfo parameterInfo,
        CommandParameterConverter converter, [NotNullWhen(true)] out Option? option)
    {
        if (CommandParameterProperties.Of(parameterInfo) is not
            { IsOption: true, Name: var name, Description: var description, Aliases: var aliases })
        {
            option = default;
            return false;
        }

        // ReSharper disable once ConstantNullCoalescingCondition
        name ??= "--" + parameterInfo.Name;

        option = new Option<object>(name, converter.Convert)
        {
            IsRequired = !parameterInfo.IsOptional,
            Description = description,
        };

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (aliases is not null)
        {
            foreach (var alias in aliases)
            {
                option.AddAlias(alias);
            }
        }

        option.AddValidator(converter.Validate);

        return true;
    }

    private static bool TryApplyExplicitArgument(EndpointParameterInfo parameterInfo,
        CommandParameterConverter converter, [NotNullWhen(true)] out Argument? argument)
    {
        if (CommandParameterProperties.Of(parameterInfo) is not
            { IsOption: false, Name: var name, Description: var description })
        {
            argument = default;
            return false;
        }

        // ReSharper disable once ConstantNullCoalescingCondition
        name ??= parameterInfo.Name;

        argument = new Argument<object>(name, converter.Convert)
        {
            Description = description
        };

        if (parameterInfo.IsOptional)
        {
            argument.Arity = ArgumentArity.ZeroOrOne;
        }

        argument.AddValidator(converter.Validate);

        return true;
    }

    private static Argument ApplyImplicitArgument(EndpointParameterInfo parameterInfo,
        CommandParameterConverter converter)
    {
        var argument = new Argument<object>(parameterInfo.Name, converter.Convert);

        if (parameterInfo.IsOptional)
        {
            argument.Arity = ArgumentArity.ZeroOrOne;
        }

        argument.AddValidator(converter.Validate);

        return argument;
    }
}
