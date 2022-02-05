using Kantaiko.CommandLine;

var program = new CommandLineProgram(args)
{
    ExecutableName = "hello-world",
    Description = "Greets world or something else"
};

return program.Run();

public class HelloController : Controller
{
    [Command]
    public string Hello([Option] string? name)
    {
        name ??= "world";

        return $"Hello, {name}!";
    }
}
