namespace Vla.Nodes.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeCategoryAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}