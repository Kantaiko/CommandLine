using Kantaiko.Controllers.ParameterConversion;
using Kantaiko.Controllers.ParameterConversion.Text;

namespace Kantaiko.CommandLine.Converters;

public class FileSystemInfoConverter : SingleTextParameterConverter<FileSystemInfo>
{
    protected override ResolutionResult<FileSystemInfo> Resolve(TextParameterConversionContext context, string value)
    {
        if (Directory.Exists(value))
        {
            return ResolutionResult.Success<FileSystemInfo>(new DirectoryInfo(value));
        }

        if (value.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
            value.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return ResolutionResult.Success<FileSystemInfo>(new DirectoryInfo(value));
        }

        return ResolutionResult.Success<FileSystemInfo>(new FileInfo(value));
    }
}
