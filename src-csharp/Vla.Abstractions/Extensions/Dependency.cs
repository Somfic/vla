namespace Vla.Abstractions.Extensions;

public readonly struct Dependency(string name, Version version)
{
    public string Name { get; init; } = name;

    public Version Version { get; init; } = version;
}