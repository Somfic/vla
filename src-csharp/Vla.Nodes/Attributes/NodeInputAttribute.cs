namespace Vla.Nodes.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class NodeInputAttribute : Attribute
{
    public NodeInputAttribute(string? name = null)
    {
        Name = name;
    }

    public string? Name { get; }
}