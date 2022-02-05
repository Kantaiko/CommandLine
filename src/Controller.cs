using System.CommandLine;
using Kantaiko.Controllers;

namespace Kantaiko.CommandLine;

public abstract class Controller : ControllerBase<CommandLineContext>
{
    protected IConsole Console => Context.InvocationContext.Console;
}
