using Kantaiko.Controllers.ParameterConversion;
using Kantaiko.Controllers.ParameterConversion.Text;

namespace Kantaiko.CommandLine.Converters;

public class DirectoryInfoConverter : SingleTextParameterConverter<DirectoryInfo>
{
    protected override ResolutionResult<DirectoryInfo> Resolve(TextParameterConversionContext context, string value)
    {
        return ResolutionResult.Success(new DirectoryInfo(value));
    }
}
