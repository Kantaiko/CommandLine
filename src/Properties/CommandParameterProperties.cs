using Kantaiko.Properties;

namespace Kantaiko.CommandLine.Properties;

public record CommandParameterProperties(
    bool IsOption,
    string? Name,
    IReadOnlyList<string>? Aliases,
    string? Description
) : ReadOnlyPropertiesBase<CommandParameterProperties>;
