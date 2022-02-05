using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Kantaiko.CommandLine.Tests;

[UsesVerify]
public class HelpTest
{
    [Theory]
    [InlineData("root", "-?")]
    [InlineData("group", "ef -?")]
    [InlineData("subgroup", "ef migrations -?")]
    [InlineData("command", "ef migrations add -?")]
    public Task ShouldRenderHelpInfo(string testName, string args)
    {
        var result = TestUtils.RunTestProgram<HelpTest>(args);

        return Verifier.Verify(result.Output)
            .DisableDiff()
            .UseDirectory("snapshots")
            .UseTextForParameters(testName);
    }

    [CommandGroup("ef", Description = "Tools for Entity Framework Core")]
    public class EfController : Controller { }

    [CommandGroup("migrations", Parent = typeof(EfController), Description = "Tools for working with migrations")]
    public class MigrationController : Controller
    {
        [Command("add", Description = "Add a new migration")]
        public void AddMigration(
            [Argument("name", Description = "Name of the migration")]
            string migrationName,
            SharedOptions options
        ) { }
    }

    public class SharedOptions
    {
        [Option("--verbose", "-v", IsOptional = true)]
        public bool Verbose { get; set; }
    }
}
