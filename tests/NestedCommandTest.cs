using Xunit;

namespace Kantaiko.CommandLine.Tests;

public class NestedCommandTest
{
    [Fact]
    public void ShouldHandleNestedCommandWithParameter()
    {
        const string args = "ef migrations add test";

        var (returnCode, output, _) = TestUtils.RunTestProgram<NestedCommandTest>(args);

        Assert.Equal(0, returnCode);
        TestUtils.AssertOutput("Migration with name test has been created.", output);
    }

    [CommandGroup("ef")]
    public class EfController : Controller { }

    [CommandGroup("migrations", Parent = typeof(EfController))]
    public class MigrationController : Controller
    {
        [Command("add")]
        public string AddMigration(string name) => $"Migration with name {name} has been created.";
    }
}
