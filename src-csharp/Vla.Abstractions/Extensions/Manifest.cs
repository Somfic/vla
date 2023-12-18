namespace Vla.Abstractions.Extensions;

public readonly struct Manifest(string name, string description, Version version)
{
    public string Name { get; init; } = name;

    public string Description { get; init; } = description;

    public Version Version { get; init; } = version;
}