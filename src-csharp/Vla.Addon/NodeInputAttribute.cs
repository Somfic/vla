namespace Vla.Addon;

[AttributeUsage(AttributeTargets.Parameter)]
public class NodeInputAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}