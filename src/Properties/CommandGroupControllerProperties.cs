using Kantaiko.Properties;

namespace Kantaiko.CommandLine.Properties;

public record CommandGroupControllerProperties(
    string GroupName,
    string? Description,
    Type? ParentType
) : ReadOnlyPropertiesBase<CommandGroupControllerProperties>;
