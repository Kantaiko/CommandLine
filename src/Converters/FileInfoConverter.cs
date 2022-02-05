using Kantaiko.Controllers.ParameterConversion;
using Kantaiko.Controllers.ParameterConversion.Text;

namespace Kantaiko.CommandLine.Converters;

public class FileInfoConverter : SingleTextParameterConverter<FileInfo>
{
    protected override ResolutionResult<FileInfo> Resolve(TextParameterConversionContext context, string value)
    {
        return ResolutionResult.Success(new FileInfo(value));
    }
}
