namespace Vla.Nodes.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class NodeOutputAttribute : Attribute
{
    public NodeOutputAttribute(string? name = null)
    {
        Name = name;
    }

    public string? Name { get; }
}