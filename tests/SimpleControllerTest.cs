using System.CommandLine;
using Xunit;

namespace Kantaiko.CommandLine.Tests;

public class SimpleControllerTest
{
    [Theory]
    [InlineData("return-code", 123, "")]
    [InlineData("return-text", 0, "hello")]
    [InlineData("write-text", 0, "hello")]
    public void ShouldHandleSimpleControllerCommand(string args, int expectedCode, string expectedOutput)
    {
        var (returnCode, output, _) = TestUtils.RunTestProgram<SimpleControllerTest>(args);

        Assert.Equal(expectedCode, returnCode);
        TestUtils.AssertOutput(expectedOutput, output);
    }

    public class TestController : Controller
    {
        [Command("return-code")]
        public int ReturnCode() => 123;

        [Command("return-text")]
        public string ReturnText() => "hello";

        [Command("write-text")]
        public void WriteText() => Console.WriteLine("hello");
    }
}
