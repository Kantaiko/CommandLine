using Xunit;

namespace Kantaiko.CommandLine.Tests;

public class RootCommandTest
{
    [Theory]
    [InlineData("", "global root")]
    [InlineData("group", "group root")]
    public void ShouldHandleRootCommand(string args, string expectedOutput)
    {
        var result = TestUtils.RunTestProgram<RootCommandTest>(args);

        TestUtils.AssertOutput(expectedOutput, result.Output);
    }

    [CommandGroup("group")]
    public class GroupController : Controller
    {
        [Command]
        public string GroupRoot() => "group root";
    }

    public class RootController : Controller
    {
        [Command]
        public string GlobalRoot() => "global root";
    }
}
