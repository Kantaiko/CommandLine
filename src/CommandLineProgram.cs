using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using Kantaiko.CommandLine.Internal;
using Kantaiko.Hosting.Modularity;
using Kantaiko.Hosting.Modularity.Introspection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kantaiko.CommandLine;

public class CommandLineProgram
{
    private readonly string[] _args;

    public CommandLineProgram(string[] args)
    {
        _args = args;

        HostBuilder = Host.CreateDefaultBuilder(args);
    }

    public string ExecutableName { get; init; } = AppDomain.CurrentDomain.FriendlyName;

    public string? Description { get; init; }

    public IConsole Console { get; init; } = new SystemConsole();

    public IHostBuilder HostBuilder { get; init; }

    public IEnumerable<Type>? LookupTypes { get; init; }

    public int Run()
    {
        HostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(sp =>
                new CommandLineHandlerAccessor(sp.GetRequiredService<HostInfo>(), LookupTypes, sp));

            services.CompleteModularityConfiguration();
        });

        var host = HostBuilder.Build();

        var introspectionInfo = host.Services.GetRequiredService<CommandLineHandlerAccessor>().IntrospectionInfo;

        using var scope = host.Services.CreateScope();

        var commandBuilder = new CommandBuilder(scope.ServiceProvider);
        var rootCommand = commandBuilder.CreateRootCommand(ExecutableName, Description, introspectionInfo);

        var builder = new CommandLineBuilder(rootCommand);

        // ReSharper disable once AccessToDisposedClosure
        builder.AddMiddleware(c => c.BindingContext.AddService(_ => scope.ServiceProvider));
        builder.UseDefaults();

        return builder.Build().Invoke(_args, Console);
    }
}
