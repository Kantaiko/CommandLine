using Kantaiko.Properties;

namespace Kantaiko.CommandLine.Properties;

public record CommandEndpointProperties(
    string? CommandName,
    string? Description
) : ReadOnlyPropertiesBase<CommandEndpointProperties>;
