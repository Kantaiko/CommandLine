using System;
using Kantaiko.Controllers.ParameterConversion;
using Kantaiko.Controllers.ParameterConversion.Text;
using Xunit;

namespace Kantaiko.CommandLine.Tests;

public class ParameterConverterTest
{
    [Theory]
    [InlineData("add test@4.2.0", 0, "Package test of version 4.2.0 was added\r\n", "")]
    [InlineData("add test", 1, null, "Invalid package reference\r\n\r\n")]
    public void ShouldHandleCommandWithParameterOfCustomType(string args, int code, string? output, string? error)
    {
        var result = TestUtils.RunTestProgram<ParameterConverterTest>(args);

        TestUtils.AssertRunResult(result, code, output, error);
    }

    public class TestController : Controller
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
        protected override ResolutionResult<PackageReference> Resolve(TextParameterConversionContext context,
            string value)
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
}
