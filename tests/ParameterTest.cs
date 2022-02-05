using Xunit;

namespace Kantaiko.CommandLine.Tests;

public class ParameterTest
{
    [Theory]
    [InlineData("add test --version 4.2.0", 0, "Package test of version 4.2.0 was added", null)]
    [InlineData("add test", 1, null, "Option '--version' is required.")]
    public void ShouldHandleCommandWithArgumentAndOption(string args, int code, string? output, string? error)
    {
        var result = TestUtils.RunTestProgram<ParameterTest>(args);

        TestUtils.AssertRunResult(result, code, output, error);
    }

    public class TestController : Controller
    {
        [Command("add")]
        public string AddPackage(string name, [Option] string version)
        {
            return $"Package {name} of version {version} was added";
        }
    }
}
