namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Parameter)]
public class NodeOutputAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}