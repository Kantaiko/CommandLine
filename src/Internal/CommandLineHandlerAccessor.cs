using System.Reflection;
using Kantaiko.CommandLine.Converters;
using Kantaiko.CommandLine.Internal.Handlers;
using Kantaiko.Controllers;
using Kantaiko.Controllers.Execution;
using Kantaiko.Controllers.Execution.Handlers;
using Kantaiko.Controllers.Introspection;
using Kantaiko.Controllers.Introspection.Factory;
using Kantaiko.Controllers.ParameterConversion.Text;
using Kantaiko.Controllers.Result;
using Kantaiko.Hosting.Modularity.Introspection;
using Kantaiko.Routing;

namespace Kantaiko.CommandLine.Internal;

internal class CommandLineHandlerAccessor
{
    public IHandler<CommandLineContext, Task<ControllerExecutionResult>> Handler { get; }

    public IntrospectionInfo IntrospectionInfo { get; }

    public CommandLineHandlerAccessor(HostInfo hostInfo, IEnumerable<Type>? controllerTypes,
        IServiceProvider services)
    {
        IEnumerable<Assembly> assemblies = hostInfo.Assemblies;

        if (Assembly.GetEntryAssembly() is { } assembly)
        {
            assemblies = assemblies.Append(assembly);
        }

        IEnumerable<Type> lookupTypes = new[]
        {
            typeof(DirectoryInfoConverter),
            typeof(FileInfoConverter),
            typeof(FileSystemInfoConverter)
        };

        lookupTypes = lookupTypes.Concat(assemblies.SelectMany(x => x.GetTypes()));

        if (controllerTypes is not null)
        {
            lookupTypes = lookupTypes.Concat(controllerTypes);
        }

        var typeArray = lookupTypes.ToArray();

        var converterCollection = new TextParameterConverterCollection(typeArray);
        var introspectionBuilder = new IntrospectionBuilder<CommandLineContext>();

        introspectionBuilder.SetServiceProvider(services);
        introspectionBuilder.AddDefaultTransformation();
        introspectionBuilder.AddTextParameterConversion(converterCollection);

        IntrospectionInfo = introspectionBuilder.CreateIntrospectionInfo(typeArray);

        var pipelineBuilder = new PipelineBuilder<CommandLineContext>();

        pipelineBuilder.AddHandler(new SetEndpointAndParametersExecutionHandler());
        pipelineBuilder.AddHandler(new ConstructParametersHandler<CommandLineContext>());
        pipelineBuilder.AddDefaultControllerHandling(new ServiceHandlerFactory());

        var handlers = pipelineBuilder.Build();

        Handler = ControllerHandlerFactory.CreateControllerHandler(IntrospectionInfo, handlers);
    }
}
