using Xunit;

namespace Kantaiko.CommandLine.Tests;

public class RootCommandTest
{
    [Theory]
    [InlineData("", "global root\r\n")]
    [InlineData("group", "group root\r\n")]
    public void ShouldHandleRootCommand(string args, string expectedOutput)
    {
        var result = TestUtils.RunTestProgram<RootCommandTest>(args);

        Assert.Equal(expectedOutput, result.Output);
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
