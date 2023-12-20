namespace Vla.Nodes.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}