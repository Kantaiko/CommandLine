Kantaiko.CommandLine
===================

![Nuget](https://img.shields.io/nuget/v/Kantaiko.CommandLine)

This library allows you to create console applications using ASP.NET-like controllers. Under the hood, it uses
[`System.CommandLine`](https://github.com/dotnet/command-line-api), a flexible and stable library from the dotnet
community. A lot of useful features like automatic help generation and autocompletion work out of the box.

## Usage

### Single root command

Thanks to the "top-level statements" feature, the simplest command line application can fit in just one file:

```c#
var program = new CommandLineProgram(args)
{
    ExecutableName = "hello-world",
    Description = "Greets world or something else"
};

return program.Run();

public class HelloController : Controller
{
    [Command]
    public string Hello(string? name)
    {
        name ??= "world";

        return $"Hello, {name}!";
    }
}
```

It contains the code to create and run the application and a single controller with a single method marked with
the `[Command]` attribute. This is the method that will be used as the handler for the single root command. Declaring
multiple such methods will result in an exception.

### Arguments and options

Each controller method (hereinafter the endpoint) can declare arguments and options. By default, all method parameters
will be treated as arguments, but this can be explicitly specified using the `[Argument]` and `[Option]`
attributes:

```c#
[Command]
public string Hello([Argument] string? name, [Option] bool verbose = false)
{
    name ??= "world";

    return $"Hello, {name}!";
}
```

By default, all arguments and options will have the same name as the corresponding parameters, and their optionality
will be determined based on their nullability or default values.

However, both of these values can be explicitly overwritten. You can also define aliases for options:

```c#
[Command]
public string Hello(
    [Argument("name", IsOptional = true)] string? name,
    [Option("--verbose", "-v", IsOptional = true)] bool verbose)
{
    name ??= "world";

    return $"Hello, {name}!";
}
```

### Return code

To specify a program return code, you can simply return it from a controller method:

```c#
var program = new CommandLineProgram(args)
{
    ExecutableName = "hello-world",
    Description = "Greets world or something else"
};

return program.Run();

public class HelloController : Controller
{
    [Command]
    public int Hello([Option] string? name)
    {
        name ??= "world";

        Console.WriteLine($"Hello, {name}!");

        return 123;
    }
}
```

In this case, you will have to use the methods of the `Console` property (not to be confused with the static
`System.Console` class) to display the text.

### Subcommands

Starting this whole game with controllers would be strange for the sake of one root command. So you can declare
subcommands by simply specifying their names in the `[Command]` attribute:

```c#
var program = new CommandLineProgram(args)
{
    ExecutableName = "calculator",
    Description = "Adds and subtracts numbers"
};

return program.Run();

public class CalculatorController : Controller
{
    [Command("add")]
    public string Add(int a, int b) => $"{a} + {b} = {a + b}";

    [Command("subtract")]
    public string Subtract(int a, int b) => $"{a} - {b} = {a - b}";
}
```

### Command groups

You can also declare nested commands. For this they must be grouped into one controller marked with the `[CommandGroup]`
attribute:

```c#
var program = new CommandLineProgram(args)
{
    ExecutableName = "dotnet",
    Description = "A fake dotnet command created as an example"
};

return program.Run();

[CommandGroup("ef", Description = "Tools for Entity Framework Core")]
public class EfController : Controller { }

[CommandGroup("migrations", Parent = typeof(EfController), Description = "Tools for working with migrations")]
public class MigrationController : Controller
{
    [Command("add", Description = "Add a new migration")]
    public string AddMigration(string name) => $"Migration named {name} has been created.";
}
```

Note that in order to nest a group within a group, you can use `Parent` property of the `[CommandGroup]` attribute.

For the command in the code above, the following help message will be generated:

```
Description:
  Add a new migration

Usage:
  dotnet ef migrations add <name> [options]

Arguments:
  <name>  Name of the migration

Options:
  -?, -h, --help           Show help and usage information
```

### Parameter composition

Composition is supposed to be used to reuse arguments and options. You can declare them as properties of a class, which
you can then simply use in a method:

```c#
public class SharedOptions
{
    [Option("--verbose", "-v", IsOptional = true)]
    public bool Verbose { get; init; }
}

[CommandGroup("migrations", Parent = typeof(EfController))]
public class MigrationController : Controller
{
    [Command("add")]
    public string AddMigration(string name, SharedOptions options)
    {
        if (options.Verbose)
        {
            Console.WriteLine("Some verbose text");
        }

        return $"Migration with name {name} has been created.";
    }
}
```

You can group parameters however you like, even creating a class for each command:

```c#
public class SharedOptions
{
    [Option("--verbose", "-v", IsOptional = true)]
    public bool Verbose { get; init; }
}

public class AddMigrationOptions
{
    public string Name { get; init; }
    public SharedOptions SharedOptions { get; init; }
}

[CommandGroup("migrations", Parent = typeof(EfController))]
public class MigrationController : Controller
{
    public string AddMigration(AddMigrationOptions options)
    {
        if (options.SharedOptions.Verbose)
        {
            Console.WriteLine("Some verbose text");
        }

        return $"Migration with name {options.Name} has been created.";
    }
}
```

### Dependency injection

And of course you can inject dependencies into controllers. Under the hood, this library uses
the [`.NET Generic host`](https://docs.microsoft.com/ru-ru/dotnet/core/extensions/generic-host), which is also
responsible for creating and configuring the dependency injection container:

```c#
var program = new CommandLineProgram(args)
{
    ExecutableName = "example",
    Description = "An example command line application"
};

program.HostBuilder.ConfigureServices(services =>
{
    // Register services here
});

return program.Run();

public class TestController : Controller
{
    private readonly IService _service;

    public TestController(IService service)
    {
        _service = service;
    }

    [Command]
    public void DoSomething()
    {
        _service.DoSomething();
    }
}
```

You can also use the `[FromServices]` attribute to inject services directly into the method:

```c#
public class TestController : Controller
{
    [Command]
    public void DoSomething([FromServices] IService service)
    {
        service.DoSomething();
    }
}
```

### Parameter converters

To declare custom parameter converters, you need to create classes that inherit from `SingleTextParameterConverter`
or `SingleAsyncTextParameterConverter`:

```c#
var program = new CommandLineProgram(args)
{
    ExecutableName = "pm",
    Description = "Yet another package manager"
};

return program.Run();

public class PackageController : Controller
{
    [Command("add")]
    public string AddPackage(PackageReference package)
    {
        return $"Package {package.Name} of version {package.Version} was added";
    }
}

public record PackageReference(string Name, Version Version);

public class PackageReferenceConverter : SingleTextParameterConverter<PackageReference>
{
    protected override ResolutionResult<PackageReference> Resolve(TextParameterConversionContext context, string value)
    {
        var parts = value.Split("@");

        if (parts.Length is not 2)
        {
            return ResolutionResult.Error("Invalid package reference");
        }

        var reference = new PackageReference(parts[0], Version.Parse(parts[1]));
        return ResolutionResult.Success(reference);
    }
}
```

In fact, the entire controller infrastructure is based on
the [`Kantaiko.Controllers`](https://github.com/Kantaiko/Controllers) library and the parameter conversion system too.

### Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

### License

[MIT](https://choosealicense.com/licenses/mit/)
